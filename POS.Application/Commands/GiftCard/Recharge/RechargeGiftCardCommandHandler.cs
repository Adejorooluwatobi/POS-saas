using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.GiftCard.Recharge;

public class RechargeGiftCardCommandHandler : IRequestHandler<RechargeGiftCardCommand, GiftCardDto>
{
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IGiftCardTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public RechargeGiftCardCommandHandler(
        IGiftCardRepository giftCardRepository,
        IGiftCardTransactionRepository transactionRepository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _giftCardRepository = giftCardRepository;
        _transactionRepository = transactionRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<GiftCardDto> Handle(RechargeGiftCardCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("No tenant context is available.");

        var tenantId = _tenantContext.TenantId.Value;
        var card = await _giftCardRepository.GetByCardNumberAsync(tenantId, request.Dto.CardNumber);
        if (card is null)
            throw new KeyNotFoundException($"Gift card with number '{request.Dto.CardNumber}' was not found.");

        if (!card.IsActive)
            throw new InvalidOperationException("This card is inactive and cannot be recharged.");

        if (card.ExpiresAt.HasValue && card.ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidOperationException("This card has expired and cannot be recharged.");

        if (request.Dto.Amount <= 0)
            throw new InvalidOperationException("Recharge amount must be greater than zero.");

        var method = PaymentMethod.Cash;
        if (!string.IsNullOrWhiteSpace(request.Dto.PaymentMethod) &&
            Enum.TryParse<PaymentMethod>(request.Dto.PaymentMethod, true, out var parsedMethod))
        {
            method = parsedMethod;
        }

        var balanceBefore = card.Balance;
        card.Balance += request.Dto.Amount;
        var balanceAfter = card.Balance;

        var tx = new GiftCardTransaction
        {
            TenantId = tenantId,
            GiftCardId = card.Id,
            Type = GiftCardTransactionType.TopUp,
            Amount = request.Dto.Amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Method = method,
            Reference = request.Dto.Reference,
            StoreId = request.Dto.StoreId ?? _tenantContext.StoreId,
            StaffId = _tenantContext.UserId,
            Notes = request.Dto.Notes ?? $"Recharge via {method}",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _transactionRepository.AddAsync(tx);
        _giftCardRepository.Update(card);
        await _uow.SaveChangesAsync(cancellationToken);

        var refreshed = await _giftCardRepository.GetByIdWithDetailsAsync(card.Id);
        return _mapper.Map<GiftCardDto>(refreshed ?? card);
    }
}
