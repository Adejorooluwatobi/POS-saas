using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Tenant.GetPaged;

public class GetTenantsPagedQueryHandler : IRequestHandler<GetTenantsPagedQuery, PagedResult<TenantDto>>
{
    private readonly ITenantRepository _repository;
    private readonly IMapper _mapper;

    public GetTenantsPagedQueryHandler(ITenantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TenantDto>> Handle(GetTenantsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<TenantDto>
        {
            Items = _mapper.Map<IEnumerable<TenantDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
