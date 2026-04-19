using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Inventory.GetById;

public class GetInventoryByIdQueryHandler : IRequestHandler<GetInventoryByIdQuery, InventoryDto?>
{
    private readonly IInventoryRepository _repository;
    private readonly IMapper _mapper;

    public GetInventoryByIdQueryHandler(IInventoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<InventoryDto?> Handle(GetInventoryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        return entity != null ? _mapper.Map<InventoryDto>(entity) : null;
    }
}
