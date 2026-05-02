using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class StoreProductOverrideRepository : GenericRepository<StoreProductOverride>, IStoreProductOverrideRepository
{
    public StoreProductOverrideRepository(RetailOsDbContext context) : base(context) { }

    public async Task<StoreProductOverride?> GetByStoreAndProductAsync(Guid storeId, Guid productId)
    {
        return await _context.StoreProductOverrides
            .FirstOrDefaultAsync(o => o.StoreId == storeId && o.ProductId == productId);
    }

    public async Task<List<StoreProductOverride>> GetByProductIdAsync(Guid productId)
    {
        return await _context.StoreProductOverrides
            .Where(o => o.ProductId == productId)
            .ToListAsync();
    }
}
