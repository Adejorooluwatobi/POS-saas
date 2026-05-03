using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Inventory.GetCrossStoreStock;

public class GetCrossStoreStockQueryHandler : IRequestHandler<GetCrossStoreStockQuery, IEnumerable<InventoryDto>>
{
    private readonly IInventoryRepository _repository;
    private readonly IMapper _mapper;

    public GetCrossStoreStockQueryHandler(IInventoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InventoryDto>> Handle(GetCrossStoreStockQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetCrossStoreStockAsync(request.VariantId);
        return _mapper.Map<IEnumerable<InventoryDto>>(entities);
    }
}
