using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Transaction.Delete;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteTransactionCommandHandler(ITransactionRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Transaction {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
