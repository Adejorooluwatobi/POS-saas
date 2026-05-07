using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Tenant.SetStatus;

public class SetTenantStatusCommandHandler : IRequestHandler<SetTenantStatusCommand>
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IUnitOfWork _uow;

    public SetTenantStatusCommandHandler(
        ITenantRepository tenantRepo,
        IStoreRepository storeRepo,
        IStaffRepository staffRepo,
        IUnitOfWork uow)
    {
        _tenantRepo = tenantRepo;
        _storeRepo = storeRepo;
        _staffRepo = staffRepo;
        _uow = uow;
    }

    public async Task Handle(SetTenantStatusCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepo.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Tenant {request.Id} not found.");

        tenant.IsActive = request.IsActive;
        _tenantRepo.Update(tenant);

        // Cascade to Stores
        var stores = await _storeRepo.GetQueryable()
            .Where(s => s.TenantId == request.Id)
            .ToListAsync(cancellationToken);
        
        foreach (var store in stores)
        {
            store.IsActive = request.IsActive;
            _storeRepo.Update(store);
        }

        // Cascade to Staff
        var staffList = await _staffRepo.GetQueryable()
            .Where(s => s.TenantId == request.Id)
            .ToListAsync(cancellationToken);
        
        foreach (var staff in staffList)
        {
            staff.IsActive = request.IsActive;
            _staffRepo.Update(staff);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
