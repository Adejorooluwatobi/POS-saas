using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using POS.Domain.Common;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Infrastructure.Repositories;

public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
{
    public InventoryRepository(RetailOsDbContext context) : base(context)
    {
    }

    public override async Task<PagedResult<Inventory>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Inventories
            .Include(i => i.Variant)
            .ThenInclude(v => v.Product);

        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PagedResult<Inventory>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Inventory?> GetByVariantAndStoreAsync(Guid variantId, Guid storeId)
    {
        var tenantContext = _context.GetService<POS.Domain.Interfaces.ITenantContext>();
        
        var result = await _context.Inventories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.VariantId == variantId && i.StoreId == storeId);
            
        if (result != null && result.TenantId == Guid.Empty && tenantContext.TenantId.HasValue)
        {
            result.TenantId = tenantContext.TenantId.Value;
        }
        
        return result;
    }

    public async Task<IEnumerable<Inventory>> GetLowStockAlertsAsync(Guid storeId)
    {
        return await _context.Inventories
            .Include(i => i.Variant)
            .ThenInclude(v => v.Product)
            .Where(i => i.StoreId == storeId && i.QuantityOnHand <= i.Variant.LowStockThreshold)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventory>> GetCrossStoreStockAsync(Guid variantId)
    {
        return await _context.Inventories
            .Include(i => i.Store)
            .Include(i => i.Variant)
            .ThenInclude(v => v.Product)
            .Where(i => i.VariantId == variantId)
            .ToListAsync();
    }
}
