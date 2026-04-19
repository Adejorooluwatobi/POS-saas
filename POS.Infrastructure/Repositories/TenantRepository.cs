using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
{
    public TenantRepository(RetailOsDbContext context) : base(context)
    {
    }
}
