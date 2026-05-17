using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Queries.GiftCard.GetByNumber;

public class GetGiftCardByNumberQueryHandler : IRequestHandler<GetGiftCardByNumberQuery, GiftCardDto?>
{
    private readonly IGiftCardRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IMapper _mapper;

    public GetGiftCardByNumberQueryHandler(IGiftCardRepository repository, ITenantContext tenantContext, IMapper mapper)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _mapper = mapper;
    }

    public async Task<GiftCardDto?> Handle(GetGiftCardByNumberQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException();
        var entity = await _repository.GetByCardNumberAsync(tenantId, request.CardNumber);
        
        return entity == null ? null : _mapper.Map<GiftCardDto>(entity);
    }
}
