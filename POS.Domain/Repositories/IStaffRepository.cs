using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IStaffRepository : IGenericRepository<Staff>
{
    Task<Staff?> GetByEmailAsync(string email);
    Task<Staff?> GetByEmployeeNoAsync(Guid storeId, string employeeNo);
    Task<Staff?> GetTenantAdminAsync(string email);
}
