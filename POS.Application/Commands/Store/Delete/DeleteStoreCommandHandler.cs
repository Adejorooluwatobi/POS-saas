using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Store.Delete;

public class DeleteStoreCommandHandler : IRequestHandler<DeleteStoreCommand>
{
    private readonly IStoreRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteStoreCommandHandler(IStoreRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Store {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
