using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
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
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<StoreDto>
        {
            Items = _mapper.Map<IEnumerable<StoreDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
