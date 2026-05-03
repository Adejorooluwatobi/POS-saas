using AutoMapper;
using MediatR;
using POS.Application.DTOs.InventoryOrder;
using POS.Domain.Repositories;

namespace POS.Application.Queries.InventoryOrder.GetById;

public class GetInventoryOrderByIdQueryHandler : IRequestHandler<GetInventoryOrderByIdQuery, InventoryOrderDto>
{
    private readonly IInventoryOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetInventoryOrderByIdQueryHandler(IInventoryOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<InventoryOrderDto> Handle(GetInventoryOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Inventory order not found.");
            
        return _mapper.Map<InventoryOrderDto>(entity);
    }
}
