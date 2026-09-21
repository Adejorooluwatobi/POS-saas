using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.GiftCard.Transfer;

public class TransferCardBalanceCommandHandler : IRequestHandler<TransferCardBalanceCommand, GiftCardDto>
{
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IGiftCardTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordService _passwordService;

    public TransferCardBalanceCommandHandler(
        IGiftCardRepository giftCardRepository,
        IGiftCardTransactionRepository transactionRepository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext,
        IPasswordService passwordService)
    {
        _giftCardRepository = giftCardRepository;
        _transactionRepository = transactionRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _passwordService = passwordService;
    }

    public async Task<GiftCardDto> Handle(TransferCardBalanceCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("No tenant context is available.");

        var tenantId = _tenantContext.TenantId.Value;

        if (string.Equals(request.Dto.SourceCardNumber.Trim(), request.Dto.DestinationCardNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Source and destination cards must be different.");

        if (request.Dto.Amount <= 0)
            throw new InvalidOperationException("Transfer amount must be greater than zero.");

        var sourceCard = await _giftCardRepository.GetByCardNumberAsync(tenantId, request.Dto.SourceCardNumber.Trim())
            ?? throw new KeyNotFoundException($"Source card '{request.Dto.SourceCardNumber}' was not found.");

        var destCard = await _giftCardRepository.GetByCardNumberAsync(tenantId, request.Dto.DestinationCardNumber.Trim())
            ?? throw new KeyNotFoundException($"Destination card '{request.Dto.DestinationCardNumber}' was not found.");

        var isSourceExpired = sourceCard.ExpiresAt.HasValue && sourceCard.ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow);
        var isDestExpired = destCard.ExpiresAt.HasValue && destCard.ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow);

        if (isDestExpired)
            throw new InvalidOperationException("Destination card has expired and cannot receive funds.");

        if (!destCard.IsActive)
            throw new InvalidOperationException("Destination card is inactive and cannot receive funds.");

        if (isSourceExpired)
        {
            // Expired card balance can be recovered via transfer, but customer must be linked first
            if (sourceCard.CustomerId is null)
            {
                throw new InvalidOperationException("Expired card must be linked to a registered customer before balance can be transferred. Please register or link the customer to this card first.");
            }
        }
        else if (!sourceCard.IsActive)
        {
            throw new InvalidOperationException("Source card is deactivated. Please activate it first before transferring funds.");
        }

        if (!string.IsNullOrEmpty(sourceCard.PinHash))
        {
            if (string.IsNullOrEmpty(request.Dto.SourcePin) || !_passwordService.Verify(request.Dto.SourcePin, sourceCard.PinHash))
            {
                throw new InvalidOperationException("Invalid source card PIN.");
            }
        }

        if (sourceCard.Balance < request.Dto.Amount)
            throw new InvalidOperationException($"Insufficient source card balance. Available: {sourceCard.Balance:N2}");

        var srcBalBefore = sourceCard.Balance;
        sourceCard.Balance -= request.Dto.Amount;
        var srcBalAfter = sourceCard.Balance;

        var dstBalBefore = destCard.Balance;
        destCard.Balance += request.Dto.Amount;
        var dstBalAfter = destCard.Balance;

        var now = DateTimeOffset.UtcNow;

        // Source transaction (debit)
        var srcTx = new GiftCardTransaction
        {
            TenantId = tenantId,
            GiftCardId = sourceCard.Id,
            Type = GiftCardTransactionType.TransferOut,
            Amount = request.Dto.Amount,
            BalanceBefore = srcBalBefore,
            BalanceAfter = srcBalAfter,
            Method = PaymentMethod.GiftCard,
            Reference = destCard.CardNumber,
            StoreId = _tenantContext.StoreId,
            StaffId = _tenantContext.UserId,
            Notes = request.Dto.Notes ?? $"Transfer to card {destCard.CardNumber}",
            CreatedAt = now
        };

        // Destination transaction (credit)
        var dstTx = new GiftCardTransaction
        {
            TenantId = tenantId,
            GiftCardId = destCard.Id,
            Type = GiftCardTransactionType.TransferIn,
            Amount = request.Dto.Amount,
            BalanceBefore = dstBalBefore,
            BalanceAfter = dstBalAfter,
            Method = PaymentMethod.GiftCard,
            Reference = sourceCard.CardNumber,
            StoreId = _tenantContext.StoreId,
            StaffId = _tenantContext.UserId,
            Notes = request.Dto.Notes ?? $"Transfer from card {sourceCard.CardNumber}",
            CreatedAt = now
        };

        await _transactionRepository.AddAsync(srcTx);
        await _transactionRepository.AddAsync(dstTx);
        _giftCardRepository.Update(sourceCard);
        _giftCardRepository.Update(destCard);

        await _uow.SaveChangesAsync(cancellationToken);

        var refreshed = await _giftCardRepository.GetByIdWithDetailsAsync(sourceCard.Id);
        return _mapper.Map<GiftCardDto>(refreshed ?? sourceCard);
    }
}
