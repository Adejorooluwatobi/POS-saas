using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Inventory.Delete;

public class DeleteInventoryCommandHandler : IRequestHandler<DeleteInventoryCommand>
{
    private readonly IInventoryRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteInventoryCommandHandler(IInventoryRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteInventoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Inventory {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
