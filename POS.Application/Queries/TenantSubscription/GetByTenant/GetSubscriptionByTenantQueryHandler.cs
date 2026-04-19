using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.TenantSubscription.GetByTenant;

public class GetSubscriptionByTenantQueryHandler : IRequestHandler<GetSubscriptionByTenantQuery, TenantSubscriptionDto?>
{
    private readonly ITenantSubscriptionRepository _repository;
    private readonly IMapper _mapper;

    public GetSubscriptionByTenantQueryHandler(ITenantSubscriptionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TenantSubscriptionDto?> Handle(GetSubscriptionByTenantQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByTenantAsync(request.TenantId);
        return entity != null ? _mapper.Map<TenantSubscriptionDto>(entity) : null;
    }
}
