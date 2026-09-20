using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.GiftCard.Redeem;

public class RedeemGiftCardCommandHandler : IRequestHandler<RedeemGiftCardCommand, GiftCardDto>
{
    private readonly IGiftCardRepository _repository;
    private readonly IGiftCardTransactionRepository _transactionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILoyaltyLedgerRepository _loyaltyLedgerRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordService _passwordService;

    public RedeemGiftCardCommandHandler(
        IGiftCardRepository repository,
        IGiftCardTransactionRepository transactionRepository,
        ICustomerRepository customerRepository,
        ITenantRepository tenantRepository,
        ILoyaltyLedgerRepository loyaltyLedgerRepository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext,
        IPasswordService passwordService)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
        _customerRepository = customerRepository;
        _tenantRepository = tenantRepository;
        _loyaltyLedgerRepository = loyaltyLedgerRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _passwordService = passwordService;
    }

    public async Task<GiftCardDto> Handle(RedeemGiftCardCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("No tenant context is available.");

        var entity = await _repository.GetByCardNumberAsync(_tenantContext.TenantId.Value, request.Dto.CardNumber)
            ?? throw new KeyNotFoundException($"Gift card '{request.Dto.CardNumber}' not found.");

        if (!entity.IsActive)
            throw new InvalidOperationException("Gift card is inactive.");

        if (entity.ExpiresAt.HasValue && entity.ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidOperationException("Gift card has expired.");

        if (!string.IsNullOrEmpty(entity.PinHash))
        {
            if (string.IsNullOrEmpty(request.Dto.Pin) || !_passwordService.Verify(request.Dto.Pin, entity.PinHash))
            {
                throw new InvalidOperationException("Invalid card PIN.");
            }
        }

        if (entity.Balance < request.Dto.Amount)
            throw new InvalidOperationException($"Insufficient balance. Available: {entity.Balance:C}.");

        var balBefore = entity.Balance;
        entity.Balance -= request.Dto.Amount;
        var balAfter = entity.Balance;

        var tx = new GiftCardTransaction
        {
            TenantId = entity.TenantId,
            GiftCardId = entity.Id,
            Type = GiftCardTransactionType.Redemption,
            Amount = request.Dto.Amount,
            BalanceBefore = balBefore,
            BalanceAfter = balAfter,
            Method = PaymentMethod.GiftCard,
            StoreId = _tenantContext.StoreId,
            StaffId = _tenantContext.UserId,
            Notes = "Direct card redemption",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _transactionRepository.AddAsync(tx);
        _repository.Update(entity);

        // ── Loyalty Points Accrual for Linked Customer ──────────────
        if (entity.CustomerId.HasValue)
        {
            var tenant = await _tenantRepository.GetByIdAsync(entity.TenantId);
            if (tenant != null && tenant.LoyaltyProgramEnabled && tenant.LoyaltyPointsEarnRate > 0)
            {
                var customer = await _customerRepository.GetByIdAsync(entity.CustomerId.Value);
                if (customer != null)
                {
                    var pointsEarned = (int)(request.Dto.Amount / tenant.LoyaltyPointsEarnRate);
                    if (pointsEarned > 0)
                    {
                        customer.PointsBalance += pointsEarned;
                        _customerRepository.Update(customer);

                        await _loyaltyLedgerRepository.AddAsync(new LoyaltyLedgerEntry
                        {
                            CustomerId = customer.Id,
                            TransactionId = null,
                            Delta = pointsEarned,
                            Reason = $"Gift Card Redemption: {entity.CardNumber}",
                            BalanceAfter = customer.PointsBalance,
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                    }
                }
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);

        var refreshed = await _repository.GetByIdWithDetailsAsync(entity.Id);
        return _mapper.Map<GiftCardDto>(refreshed ?? entity);
    }
}
