using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class StaffRepository : GenericRepository<Staff>, IStaffRepository
{
    public StaffRepository(RetailOsDbContext context) : base(context) { }

    public async Task<Staff?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(s => string.Equals(s.Email, email) && s.IsActive);
    }

    public async Task<Staff?> GetByEmployeeNoAsync(Guid storeId, string employeeNo)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.StoreId == storeId && string.Equals(s.EmployeeNo, employeeNo) && s.IsActive);
    }

    public async Task<Staff?> GetTenantAdminAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(s => string.Equals(s.Email, email) && s.SystemRole == Domain.Enums.SystemRole.TenantAdmin && s.IsActive);
    }
}
