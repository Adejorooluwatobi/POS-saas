using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class StoreRepository : GenericRepository<Store>, IStoreRepository
{
    public StoreRepository(RetailOsDbContext context) : base(context)
    {
    }

    public override async Task<Store?> GetByIdAsync(Guid id)
    {
        return await _dbSet.Include(s => s.Tenant).FirstOrDefaultAsync(s => s.Id == id);
    }
}

