using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface ITenantSubscriptionRepository : IGenericRepository<TenantSubscription>
{
    Task<TenantSubscription?> GetByTenantAsync(Guid tenantId);
}
