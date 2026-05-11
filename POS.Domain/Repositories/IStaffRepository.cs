using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IStaffRepository : IGenericRepository<Staff>
{
    Task<Staff?> GetByEmailAsync(string email, Guid? tenantId = null);
    Task<Staff?> GetByEmployeeNoAsync(Guid storeId, string employeeNo);
    Task<Staff?> GetTenantAdminAsync(string email, Guid? tenantId = null);
    Task<IEnumerable<Staff>> GetManagersByStoreAsync(Guid storeId);
    Task<IEnumerable<Staff>> GetGeneralsAsync();
}
