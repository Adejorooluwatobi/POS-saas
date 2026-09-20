using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.GiftCard.ReplaceLost;

public class ReplaceLostCardCommandHandler : IRequestHandler<ReplaceLostCardCommand, GiftCardDto>
{
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IGiftCardTransactionRepository _transactionRepository;
    private readonly IGiftCardNumberGenerator _cardNumberGenerator;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public ReplaceLostCardCommandHandler(
        IGiftCardRepository giftCardRepository,
        IGiftCardTransactionRepository transactionRepository,
        IGiftCardNumberGenerator cardNumberGenerator,
        IPasswordService passwordService,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _giftCardRepository = giftCardRepository;
        _transactionRepository = transactionRepository;
        _cardNumberGenerator = cardNumberGenerator;
        _passwordService = passwordService;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<GiftCardDto> Handle(ReplaceLostCardCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("No tenant context is available.");

        var tenantId = _tenantContext.TenantId.Value;
        var lostCard = await _giftCardRepository.GetByCardNumberAsync(tenantId, request.Dto.LostCardNumber.Trim())
            ?? throw new KeyNotFoundException($"Lost card '{request.Dto.LostCardNumber}' was not found.");

        var isExpired = lostCard.ExpiresAt.HasValue && lostCard.ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow);

        // Strict rule: If expired card has no customer linked, customer registration is mandatory first
        if (isExpired && lostCard.CustomerId is null)
        {
            throw new InvalidOperationException("Expired card must be linked to a registered customer before balance can be transferred. Please register or link the customer to this card first.");
        }

        // Verification check
        if (!request.Dto.BypassVerification && !string.IsNullOrEmpty(lostCard.PinHash))
        {
            if (string.IsNullOrEmpty(request.Dto.VerificationPin) || !_passwordService.Verify(request.Dto.VerificationPin, lostCard.PinHash))
            {
                throw new InvalidOperationException("Invalid verification PIN for the lost card.");
            }
        }

        // Determine new card number
        var newCardNumber = !string.IsNullOrWhiteSpace(request.Dto.NewCardNumber)
            ? request.Dto.NewCardNumber.Trim()
            : await _cardNumberGenerator.GenerateCardNumberAsync(tenantId, cancellationToken);

        // Determine PIN
        var rawPin = !string.IsNullOrWhiteSpace(request.Dto.NewCardPin)
            ? request.Dto.NewCardPin.Trim()
            : new Random().Next(1000, 9999).ToString();
        var newPinHash = _passwordService.Hash(rawPin);

        var balanceToMigrate = lostCard.Balance;

        // 1. Deactivate old card permanently
        lostCard.IsActive = false;
        lostCard.Notes = $"Deactivated: Reported lost/misplaced. Replaced by card {newCardNumber}. Reason: {request.Dto.Reason ?? "Card Replacement"}";

        // 2. Create new card
        var newCard = new POS.Domain.Entities.GiftCard
        {
            TenantId = tenantId,
            CardNumber = newCardNumber,
            PinHash = newPinHash,
            Balance = balanceToMigrate,
            InitialValue = balanceToMigrate,
            CustomerId = lostCard.CustomerId,
            IssuingStoreId = _tenantContext.StoreId ?? lostCard.IssuingStoreId,
            IsActive = request.Dto.ActivateNewCard,
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            Notes = $"Issued as replacement for lost card {lostCard.CardNumber}. {(request.Dto.BypassVerification ? $"[Verification Bypassed: {request.Dto.BypassReason}]" : "")}",
            IssuedAt = DateTimeOffset.UtcNow
        };

        await _giftCardRepository.AddAsync(newCard);
        await _uow.SaveChangesAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;

        // 3. Financial audit trail
        if (balanceToMigrate > 0)
        {
            var debitTx = new GiftCardTransaction
            {
                TenantId = tenantId,
                GiftCardId = lostCard.Id,
                Type = GiftCardTransactionType.TransferOut,
                Amount = balanceToMigrate,
                BalanceBefore = balanceToMigrate,
                BalanceAfter = 0,
                Method = PaymentMethod.GiftCard,
                Reference = newCardNumber,
                StoreId = _tenantContext.StoreId,
                StaffId = _tenantContext.UserId,
                Notes = $"Balance migrated to replacement card {newCardNumber}",
                CreatedAt = now
            };

            var creditTx = new GiftCardTransaction
            {
                TenantId = tenantId,
                GiftCardId = newCard.Id,
                Type = GiftCardTransactionType.TransferIn,
                Amount = balanceToMigrate,
                BalanceBefore = 0,
                BalanceAfter = balanceToMigrate,
                Method = PaymentMethod.GiftCard,
                Reference = lostCard.CardNumber,
                StoreId = _tenantContext.StoreId,
                StaffId = _tenantContext.UserId,
                Notes = $"Balance migrated from lost card {lostCard.CardNumber}",
                CreatedAt = now
            };

            lostCard.Balance = 0;
            await _transactionRepository.AddAsync(debitTx);
            await _transactionRepository.AddAsync(creditTx);
        }

        _giftCardRepository.Update(lostCard);
        _giftCardRepository.Update(newCard);
        await _uow.SaveChangesAsync(cancellationToken);

        var refreshed = await _giftCardRepository.GetByIdWithDetailsAsync(newCard.Id);
        return _mapper.Map<GiftCardDto>(refreshed ?? newCard);
    }
}
