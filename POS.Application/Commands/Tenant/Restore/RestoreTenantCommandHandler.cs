using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Tenant.Restore;

public class RestoreTenantCommandHandler : IRequestHandler<RestoreTenantCommand>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _uow;

    public RestoreTenantCommandHandler(ITenantRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(RestoreTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {request.Id} not found in the recycle bin.");

        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.IsActive = true;

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
