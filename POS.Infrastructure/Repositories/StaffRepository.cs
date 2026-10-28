using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class StaffRepository : GenericRepository<Staff>, IStaffRepository
{
    public StaffRepository(RetailOsDbContext context) : base(context) { }

    public async Task<Staff?> GetByEmailAsync(string email, Guid? tenantId = null)
    {
        var normalized = email.Trim().ToLower();
        return await _dbSet.FirstOrDefaultAsync(s =>
            s.Email.ToLower() == normalized &&
            (tenantId == null || s.TenantId == tenantId));
    }

    public async Task<Staff?> GetByEmployeeNoAsync(Guid storeId, string employeeNo)
    {
        var normalized = employeeNo.Trim().ToLower();
        return await _dbSet.FirstOrDefaultAsync(s =>
            s.StoreId == storeId &&
            s.EmployeeNo.ToLower() == normalized);
    }

    public async Task<Staff?> GetTenantAdminAsync(string email, Guid? tenantId = null)
    {
        var normalized = email.Trim().ToLower();
        return await _dbSet.FirstOrDefaultAsync(s =>
            s.Email.ToLower() == normalized &&
            s.SystemRole == Domain.Enums.SystemRole.TenantAdmin &&
            (tenantId == null || s.TenantId == tenantId));
    }

    public async Task<IEnumerable<Staff>> GetManagersByStoreAsync(Guid storeId)
    {
        return await _dbSet
            .Where(s => s.StoreId == storeId && (s.SystemRole == Domain.Enums.SystemRole.StoreManager || s.SystemRole == Domain.Enums.SystemRole.Manager))
            .ToListAsync();
    }

    public async Task<IEnumerable<Staff>> GetGeneralsAsync()
    {
        return await _dbSet
            .Where(s => s.SystemRole == Domain.Enums.SystemRole.SuperAdmin || 
                        s.SystemRole == Domain.Enums.SystemRole.TenantAdmin || 
                        s.SystemRole == Domain.Enums.SystemRole.Manager)
            .ToListAsync();
    }
}
