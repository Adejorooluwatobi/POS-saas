using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs.InventoryOrder;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.InventoryOrder.GetPaged;

public class GetInventoryOrdersPagedQueryHandler : IRequestHandler<GetInventoryOrdersPagedQuery, PagedResult<InventoryOrderDto>>
{
    private readonly IInventoryOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetInventoryOrdersPagedQueryHandler(IInventoryOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<InventoryOrderDto>> Handle(GetInventoryOrdersPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<InventoryOrderDto>
        {
            Items = _mapper.Map<IEnumerable<InventoryOrderDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
