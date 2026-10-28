using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Staff.Delete;

public class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand>
{
    private readonly IStaffRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public DeleteStaffCommandHandler(IStaffRepository repository, IUnitOfWork uow, ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Staff {request.Id} not found.");

        if (_tenantContext.SystemRole == "Supervisor")
        {
            throw new UnauthorizedAccessException("Supervisors do not have permission to delete staff. Please contact a Manager or Admin.");
        }

        if (_tenantContext.SystemRole is "StoreManager" or "Manager")
        {
            if (_tenantContext.StoreId.HasValue && entity.StoreId != _tenantContext.StoreId)
            {
                throw new UnauthorizedAccessException("Store Managers can only delete staff members belonging to their assigned store.");
            }
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.IsActive = false;

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
