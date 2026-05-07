using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var query = _repository.GetQueryable()
            .Include(t => t.Staff)
            .OrderByDescending(t => t.CreatedAt);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<TenantDto>
        {
            Items = _mapper.Map<IEnumerable<TenantDto>>(items),
            TotalCount = count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
