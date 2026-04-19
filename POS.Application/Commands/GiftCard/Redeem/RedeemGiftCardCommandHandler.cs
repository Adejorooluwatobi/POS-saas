using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.GiftCard.Redeem;

public class RedeemGiftCardCommandHandler : IRequestHandler<RedeemGiftCardCommand, GiftCardDto>
{
    private readonly IGiftCardRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public RedeemGiftCardCommandHandler(
        IGiftCardRepository repository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
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

        if (entity.Balance < request.Dto.Amount)
            throw new InvalidOperationException($"Insufficient balance. Available: {entity.Balance:C}.");

        entity.Balance -= request.Dto.Amount;

        if (entity.Balance == 0)
            entity.IsActive = false;

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<GiftCardDto>(entity);
    }
}
