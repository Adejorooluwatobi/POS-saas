using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Store.GetPaged;

public class GetStoresPagedQueryHandler : IRequestHandler<GetStoresPagedQuery, PagedResult<StoreDto>>
{
    private readonly IStoreRepository _repository;
    private readonly IMapper _mapper;

    public GetStoresPagedQueryHandler(IStoreRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<StoreDto>> Handle(GetStoresPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetQueryable();

        if (request.TenantId.HasValue)
        {
            query = query.Where(s => s.TenantId == request.TenantId.Value);
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<StoreDto>
        {
            Items = _mapper.Map<IEnumerable<StoreDto>>(items),
            TotalCount = count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
