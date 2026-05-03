using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Common;

namespace POS.Infrastructure.Repositories;

public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
{
    public InventoryRepository(RetailOsDbContext context) : base(context)
    {
    }

    public override async Task<PagedResult<Inventory>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Inventories
            .Include(i => i.Variant);

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
        return await _context.Inventories
            .FirstOrDefaultAsync(i => i.VariantId == variantId && i.StoreId == storeId);
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
            .Where(i => i.VariantId == variantId)
            .ToListAsync();
    }
}
