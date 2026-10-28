using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Staff.Purge;

public class PurgeStaffCommandHandler : IRequestHandler<PurgeStaffCommand>
{
    private readonly IStaffRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public PurgeStaffCommandHandler(IStaffRepository repository, IUnitOfWork uow, ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(PurgeStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Staff {request.Id} not found in the recycle bin.");

        if (entity.Id == Guid.Parse("00000000-0000-0000-0000-000000000002"))
        {
            throw new InvalidOperationException("The root system administrator cannot be deleted.");
        }

        if (_tenantContext.SystemRole != "SuperAdmin")
        {
            if (_tenantContext.SystemRole is not "TenantAdmin")
            {
                throw new UnauthorizedAccessException("Only Tenant Administrators can permanently purge staff records.");
            }

            if (entity.TenantId != _tenantContext.TenantId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete staff from another organization.");
            }
        }

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
