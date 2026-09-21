using AutoMapper;
using MediatR;
using POS.Application.DTOs.StockRequisition;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Queries.StockRequisition.GetById;

public class GetStockRequisitionByIdQueryHandler : IRequestHandler<GetStockRequisitionByIdQuery, StockRequisitionDto>
{
    private readonly IStockRequisitionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetStockRequisitionByIdQueryHandler(IStockRequisitionRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<StockRequisitionDto> Handle(GetStockRequisitionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Stock requisition not found.");

        // Auto-heal: If all fulfillment orders are Approved/Resolved and none are Disputed, promote PartiallyFulfilled to FullyFulfilled
        if (entity.Status == RequisitionStatus.PartiallyFulfilled && entity.FulfillmentOrders.Count > 0)
        {
            var anyPending = entity.FulfillmentOrders.Any(o => 
                o.Status == InventoryOrderStatus.Draft || 
                o.Status == InventoryOrderStatus.Dispatched || 
                o.Status == InventoryOrderStatus.Received);
            var anyDisputed = entity.FulfillmentOrders.Any(o => o.Status == InventoryOrderStatus.Disputed);

            if (!anyPending && !anyDisputed)
            {
                entity.Status = RequisitionStatus.FullyFulfilled;
                await _uow.SaveChangesAsync(cancellationToken);
            }
        }
            
        return _mapper.Map<StockRequisitionDto>(entity);
    }
}
