using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Inventory.Update;

public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand>
{
    private readonly IInventoryRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateInventoryCommandHandler(IInventoryRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Inventory {request.Id} not found.");

        _mapper.Map(request.Dto, entity);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
