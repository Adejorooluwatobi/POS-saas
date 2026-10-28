using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Staff.Restore;

public class RestoreStaffCommandHandler : IRequestHandler<RestoreStaffCommand>
{
    private readonly IStaffRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public RestoreStaffCommandHandler(IStaffRepository repository, IUnitOfWork uow, ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(RestoreStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Staff member {request.Id} not found in the recycle bin.");

        if (_tenantContext.SystemRole != "SuperAdmin")
        {
            if (entity.TenantId != _tenantContext.TenantId)
                throw new UnauthorizedAccessException("You do not have permission to restore staff in another organization.");

            if (_tenantContext.SystemRole is "StoreManager" && _tenantContext.StoreId.HasValue && entity.StoreId != _tenantContext.StoreId)
                throw new UnauthorizedAccessException("Store Managers can only restore staff members belonging to their assigned store.");
        }

        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.IsActive = true;

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
