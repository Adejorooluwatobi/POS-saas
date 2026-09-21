using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Enums;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.GiftCard;

namespace POS.Application.Commands.GiftCard.Issue;

public class IssueGiftCardCommandHandler : IRequestHandler<IssueGiftCardCommand, GiftCardDto>
{
    private readonly IGiftCardRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IGiftCardNumberGenerator _cardNumberGenerator;
    private readonly IPasswordService _passwordService;

    public IssueGiftCardCommandHandler(
        IGiftCardRepository repository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext,
        IGiftCardNumberGenerator cardNumberGenerator,
        IPasswordService passwordService)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _cardNumberGenerator = cardNumberGenerator;
        _passwordService = passwordService;
    }

    public async Task<GiftCardDto> Handle(IssueGiftCardCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("No tenant context is available.");

        var entity = _mapper.Map<Entity>(request.Dto);
        entity.TenantId = _tenantContext.TenantId.Value;
        entity.CardNumber = await _cardNumberGenerator.GenerateCardNumberAsync(entity.TenantId, cancellationToken);
        entity.Balance = request.Dto.InitialValue;
        entity.IsActive = request.Dto.ActivateNow;
        entity.IssuedAt = DateTimeOffset.UtcNow;
        entity.CustomerId = request.Dto.CustomerId;

        if (!string.IsNullOrEmpty(request.Dto.Pin))
        {
            entity.PinHash = _passwordService.Hash(request.Dto.Pin);
        }

        // Scoping Logic: Track where the gift card was issued
        if (_tenantContext.SystemRole is "StoreManager" or "Supervisor" or "Cashier")
        {
            entity.IssuingStoreId = _tenantContext.StoreId;
        }
        else
        {
            entity.IssuingStoreId = request.Dto.IssuingStoreId;
        }

        if (request.Dto.InitialValue > 0)
        {
            var paymentMethod = PaymentMethod.Cash;
            if (!string.IsNullOrEmpty(request.Dto.PaymentMethod) && Enum.TryParse<PaymentMethod>(request.Dto.PaymentMethod, true, out var parsedMethod))
            {
                paymentMethod = parsedMethod;
            }

            entity.Transactions.Add(new GiftCardTransaction
            {
                TenantId = entity.TenantId,
                GiftCardId = entity.Id,
                Type = GiftCardTransactionType.Issuance,
                Amount = request.Dto.InitialValue,
                BalanceBefore = 0,
                BalanceAfter = request.Dto.InitialValue,
                Method = paymentMethod,
                Reference = request.Dto.Reference,
                StoreId = entity.IssuingStoreId,
                StaffId = _tenantContext.UserId,
                Notes = "Card issuance initial value",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        // Fetch back with details (customer, store)
        var detailed = await _repository.GetByIdWithDetailsAsync(entity.Id);
        return _mapper.Map<GiftCardDto>(detailed ?? entity);
    }
}
