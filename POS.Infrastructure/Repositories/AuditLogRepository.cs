using Microsoft.EntityFrameworkCore;
using POS.Domain.Common;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(RetailOsDbContext context) : base(context) { }

    public async Task<PagedResult<AuditLog>> GetPagedByTenantAsync(Guid tenantId, int pageNumber, int pageSize)
    {
        var query = _dbSet.Where(a => a.TenantId == tenantId).OrderByDescending(a => a.CreatedAt);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
