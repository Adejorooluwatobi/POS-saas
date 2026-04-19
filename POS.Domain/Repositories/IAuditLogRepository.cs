using POS.Domain.Common;
using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<PagedResult<AuditLog>> GetPagedByTenantAsync(Guid tenantId, int pageNumber, int pageSize);
}
