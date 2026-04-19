using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Inventory;

namespace POS.Application.Commands.Inventory.Create;

public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand, InventoryDto>
{
    private readonly IInventoryRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateInventoryCommandHandler(IInventoryRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<InventoryDto> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<InventoryDto>(entity);
    }
}
