using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Transaction.Create;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly ITransactionRepository _repository;
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IReceiptNumberService _receiptNumberService;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly ITillSessionRepository _sessionRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IPasswordService _passwordService;

    public CreateTransactionCommandHandler(
        ITransactionRepository repository, IStoreRepository storeRepository,
        IUnitOfWork uow, IMapper mapper,
        ITenantContext tenantContext, IReceiptNumberService receiptNumberService,
        IInventoryRepository inventoryRepository, IProductVariantRepository variantRepository,
        ITillSessionRepository sessionRepository,
        IGiftCardRepository giftCardRepository,
        IPasswordService passwordService)
    {
        _repository = repository;
        _storeRepository = storeRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _receiptNumberService = receiptNumberService;
        _inventoryRepository = inventoryRepository;
        _variantRepository = variantRepository;
        _sessionRepository = sessionRepository;
        _giftCardRepository = giftCardRepository;
        _passwordService = passwordService;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var store = await _storeRepository.GetByIdAsync(request.Dto.StoreId)
            ?? throw new KeyNotFoundException($"Store {request.Dto.StoreId} not found.");

        var session = await _sessionRepository.GetByIdAsync(request.Dto.SessionId)
            ?? throw new KeyNotFoundException($"Till Session {request.Dto.SessionId} not found.");

        if (session.Status != POS.Domain.Enums.SessionStatus.Open)
            throw new InvalidOperationException("Cannot perform transaction on a closed till session.");

        var entity = _mapper.Map<POS.Domain.Entities.Transaction>(request.Dto);
        entity.CashierId = _tenantContext.UserId!.Value;
        entity.ReceiptNumber = _receiptNumberService.Generate(store.Code);

        foreach (var itemDto in request.Dto.Items)
        {
            var taxAmount = itemDto.UnitPrice * itemDto.Quantity * itemDto.TaxRate;
            var lineTotal = itemDto.UnitPrice * itemDto.Quantity + taxAmount;

            // ── Inventory Deduction & Name Resolution ────────────────────────
            var variant = await _variantRepository.GetByIdAsync(itemDto.VariantId);
            var itemName = itemDto.Name;
            
            if (string.IsNullOrEmpty(itemName))
            {
                if (itemDto.IsGiftCardSale)
                {
                    itemName = !string.IsNullOrEmpty(itemDto.GiftCardNumber) 
                        ? $"Gift Card ({itemDto.GiftCardNumber})" 
                        : "Gift Card Sale";
                }
                else if (variant != null)
                {
                    itemName = variant.Product?.Name ?? "Unknown";
                }
            }

            entity.Items.Add(new TransactionItem
            {
                TransactionId = entity.Id,
                VariantId = itemDto.VariantId == Guid.Empty ? null : itemDto.VariantId,
                ProductName = itemName ?? "Unknown",
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                OriginalPrice = itemDto.UnitPrice,
                TaxRate = itemDto.TaxRate,
                TaxAmount = taxAmount,
                LineTotal = lineTotal
            });

            if (variant != null)
            {
                var baseVariantId = variant.IsBaseUnit ? variant.Id : variant.BaseVariantId!.Value;
                var qtyInBaseUnits = (int)(itemDto.Quantity * variant.ConversionFactor);

                var inventory = await _inventoryRepository.GetByVariantAndStoreAsync(baseVariantId, request.Dto.StoreId);
                if (inventory != null)
                {
                    inventory.QuantityOnHand -= qtyInBaseUnits;
                }
                // Optional: Handle out-of-stock scenarios or logs
            }

            // ── Gift Card Issuance / Top-up ──────────────────────────────────
            if (itemDto.IsGiftCardSale && !string.IsNullOrEmpty(itemDto.GiftCardNumber))
            {
                var existingCard = await _giftCardRepository.GetByCardNumberAsync(_tenantContext.TenantId!.Value, itemDto.GiftCardNumber);
                if (existingCard != null)
                {
                    // If it exists, we just add the balance (Top-up or Activation)
                    existingCard.Balance += lineTotal;
                    existingCard.IsActive = true; // Ensure it's active
                    if (existingCard.IssuingStoreId == null) existingCard.IssuingStoreId = request.Dto.StoreId;
                    
                    if (!string.IsNullOrEmpty(itemDto.GiftCardPin))
                    {
                        if (string.IsNullOrEmpty(existingCard.PinHash))
                        {
                            existingCard.PinHash = _passwordService.Hash(itemDto.GiftCardPin);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(itemDto.GiftCardOldPin) || !_passwordService.Verify(itemDto.GiftCardOldPin, existingCard.PinHash))
                            {
                                throw new InvalidOperationException("Invalid current PIN for Gift Card");
                            }
                            existingCard.PinHash = _passwordService.Hash(itemDto.GiftCardPin);
                        }
                    }

                    _giftCardRepository.Update(existingCard);
                }
                else
                {
                    // If it doesn't exist, create it
                    var giftCard = new POS.Domain.Entities.GiftCard
                    {
                        TenantId = _tenantContext.TenantId!.Value,
                        CardNumber = itemDto.GiftCardNumber,
                        Balance = lineTotal,
                        InitialValue = lineTotal,
                        PinHash = !string.IsNullOrEmpty(itemDto.GiftCardPin) 
                            ? _passwordService.Hash(itemDto.GiftCardPin) 
                            : null,
                        IsActive = true,
                        IssuedAt = DateTimeOffset.UtcNow,
                        IssuingStoreId = request.Dto.StoreId
                    };
                    await _giftCardRepository.AddAsync(giftCard);
                }
            }
        }

        entity.Subtotal = entity.Items.Sum(i => i.UnitPrice * i.Quantity);
        entity.TaxTotal = entity.Items.Sum(i => i.TaxAmount);
        entity.GrandTotal = entity.Subtotal + entity.TaxTotal - entity.DiscountTotal;

        // ── Payment Processing ───────────────────────────────────────────
        if (request.Dto.Payments != null && request.Dto.Payments.Any())
        {
            foreach (var p in request.Dto.Payments)
            {
                var paymentEntity = new POS.Domain.Entities.Payment
                {
                    TransactionId = entity.Id,
                    Method = p.Method,
                    GiftCardId = p.GiftCardId,
                    Amount = p.Amount,
                    AmountTendered = p.AmountTendered,
                    ChangeGiven = (p.AmountTendered ?? p.Amount) - p.Amount,
                    Status = POS.Domain.Enums.PaymentStatus.Approved,
                    ProcessedAt = DateTimeOffset.UtcNow
                };
                entity.Payments.Add(paymentEntity);
                entity.AmountPaid += p.Amount;
                entity.ChangeGiven += paymentEntity.ChangeGiven ?? 0m;
            }

            if (entity.AmountPaid >= entity.GrandTotal)
            {
                entity.Status = POS.Domain.Enums.TransactionStatus.Completed;
                entity.CompletedAt = DateTimeOffset.UtcNow;
            }
        }

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        // Fetch back with navigation properties to ensure Cashier and Store names are populated for the DTO mapping
        var result = await _repository.GetByIdAsync(entity.Id);
        return _mapper.Map<TransactionDto>(result ?? entity);
    }
}
