using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.TillSession.Delete;

public class DeleteTillSessionCommandHandler : IRequestHandler<DeleteTillSessionCommand>
{
    private readonly ITillSessionRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteTillSessionCommandHandler(ITillSessionRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteTillSessionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"TillSession {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
