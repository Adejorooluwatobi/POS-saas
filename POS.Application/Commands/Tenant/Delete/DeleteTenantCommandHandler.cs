using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Tenant.Delete;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteTenantCommandHandler(ITenantRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Tenant {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
