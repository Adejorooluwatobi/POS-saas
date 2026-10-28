using System;
using System.Threading;
using System.Threading.Tasks;
using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface ITenantRepository : IGenericRepository<Tenant>
{
    Task PurgeTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
