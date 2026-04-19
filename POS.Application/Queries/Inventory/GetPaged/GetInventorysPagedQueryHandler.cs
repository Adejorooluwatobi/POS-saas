using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Inventory.GetPaged;

public class GetInventorysPagedQueryHandler : IRequestHandler<GetInventorysPagedQuery, PagedResult<InventoryDto>>
{
    private readonly IInventoryRepository _repository;
    private readonly IMapper _mapper;

    public GetInventorysPagedQueryHandler(IInventoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<InventoryDto>> Handle(GetInventorysPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<InventoryDto>
        {
            Items = _mapper.Map<IEnumerable<InventoryDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
