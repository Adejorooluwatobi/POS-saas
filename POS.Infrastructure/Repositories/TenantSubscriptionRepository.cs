using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class TenantSubscriptionRepository : GenericRepository<TenantSubscription>, ITenantSubscriptionRepository
{
    public TenantSubscriptionRepository(RetailOsDbContext context) : base(context) { }

    public async Task<TenantSubscription?> GetByTenantAsync(Guid tenantId) =>
        await _dbSet.FirstOrDefaultAsync(s => s.TenantId == tenantId);
}
