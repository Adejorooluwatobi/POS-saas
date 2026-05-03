using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Inventory.GetLowStockAlerts;

public class GetLowStockAlertsQueryHandler : IRequestHandler<GetLowStockAlertsQuery, IEnumerable<InventoryDto>>
{
    private readonly IInventoryRepository _repository;
    private readonly IMapper _mapper;

    public GetLowStockAlertsQueryHandler(IInventoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InventoryDto>> Handle(GetLowStockAlertsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetLowStockAlertsAsync(request.StoreId);
        return _mapper.Map<IEnumerable<InventoryDto>>(entities);
    }
}
