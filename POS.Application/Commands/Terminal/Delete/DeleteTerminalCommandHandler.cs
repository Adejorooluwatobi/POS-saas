using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Terminal.Delete;

public class DeleteTerminalCommandHandler : IRequestHandler<DeleteTerminalCommand>
{
    private readonly ITerminalRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteTerminalCommandHandler(ITerminalRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteTerminalCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Terminal {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
