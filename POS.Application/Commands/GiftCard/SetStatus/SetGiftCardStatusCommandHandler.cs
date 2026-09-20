using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.GiftCard.SetStatus;

public class SetGiftCardStatusCommandHandler : IRequestHandler<SetGiftCardStatusCommand, GiftCardDto>
{
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public SetGiftCardStatusCommandHandler(
        IGiftCardRepository giftCardRepository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _giftCardRepository = giftCardRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<GiftCardDto> Handle(SetGiftCardStatusCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("No tenant context is available.");

        var card = await _giftCardRepository.GetByIdWithDetailsAsync(request.CardId)
            ?? throw new KeyNotFoundException($"Gift card with ID '{request.CardId}' was not found.");

        if (card.TenantId != _tenantContext.TenantId.Value)
            throw new UnauthorizedAccessException("Cannot modify card belonging to another tenant.");

        if (request.Dto.IsActive)
        {
            // Strict rule: Expired cards can NEVER be reactivated
            if (card.ExpiresAt.HasValue && card.ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new InvalidOperationException("Expired cards can NEVER be reactivated. Balance can only be transferred to a new replacement card.");
            }

            card.IsActive = true;
            if (!string.IsNullOrWhiteSpace(request.Dto.Reason))
            {
                card.Notes = request.Dto.Reason;
            }
        }
        else
        {
            card.IsActive = false;
            card.Notes = request.Dto.Reason ?? "Deactivated by staff";
        }

        _giftCardRepository.Update(card);
        await _uow.SaveChangesAsync(cancellationToken);

        var refreshed = await _giftCardRepository.GetByIdWithDetailsAsync(card.Id);
        return _mapper.Map<GiftCardDto>(refreshed ?? card);
    }
}
