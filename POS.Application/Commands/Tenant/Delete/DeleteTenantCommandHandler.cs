using System;
using System.Threading;
using System.Threading.Tasks;
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

        if (entity.Slug.Equals("system", StringComparison.OrdinalIgnoreCase) || 
            entity.Id == Guid.Parse("00000000-0000-0000-0000-000000000001"))
        {
            throw new InvalidOperationException("The root system tenant cannot be deleted or moved to the recycle bin.");
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.IsActive = false;

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
