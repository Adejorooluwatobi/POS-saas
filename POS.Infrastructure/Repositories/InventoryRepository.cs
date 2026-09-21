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

    public override async Task<Inventory?> GetByIdAsync(Guid id)
    {
        return await _context.Inventories
            .Include(i => i.Store)
            .Include(i => i.Variant)
                .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<PagedResult<AggregatedInventory>> GetAggregatedPagedAsync(int pageNumber, int pageSize)
    {
        var tenantContext = _context.GetService<POS.Domain.Interfaces.ITenantContext>();

        var groupedQuery = _context.Inventories
            .GroupBy(i => i.VariantId)
            .Select(g => new
            {
                VariantId = g.Key,
                QuantityOnHand = g.Sum(i => i.QuantityOnHand),
                QuantityReserved = g.Sum(i => i.QuantityReserved),
                ReorderPoint = g.Sum(i => i.ReorderPoint),
                ReorderQty = g.Sum(i => i.ReorderQty),
                UpdatedAt = g.Max(i => i.UpdatedAt),
                FirstInventoryId = g.Select(i => i.Id).FirstOrDefault(),
                StoreCount = g.Select(i => i.StoreId).Distinct().Count()
            });

        var count = await groupedQuery.CountAsync();

        var pagedData = await groupedQuery
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var variantIds = pagedData.Select(x => x.VariantId).ToList();

        var variants = await _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

        string defaultStoreName = "All Stores";
        if (tenantContext.StoreId.HasValue)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == tenantContext.StoreId.Value);
            if (store != null)
            {
                defaultStoreName = store.Name;
            }
        }

        var items = pagedData.Select(x =>
        {
            variants.TryGetValue(x.VariantId, out var variant);
            return new AggregatedInventory
            {
                Id = x.FirstInventoryId,
                VariantId = x.VariantId,
                VariantName = variant?.Product?.Name ?? variant?.Sku ?? "Unknown Product",
                Sku = variant?.Sku ?? string.Empty,
                StoreId = tenantContext.StoreId ?? Guid.Empty,
                StoreName = tenantContext.StoreId.HasValue ? defaultStoreName : (x.StoreCount > 1 ? "All Stores" : defaultStoreName),
                QuantityOnHand = x.QuantityOnHand,
                QuantityReserved = x.QuantityReserved,
                ReorderPoint = x.ReorderPoint,
                ReorderQty = x.ReorderQty,
                SinglesPerRoll = variant?.Product?.SinglesPerRoll,
                RollsPerPack = variant?.Product?.RollsPerPack,
                SinglesPerPack = variant?.Product?.SinglesPerPack,
                UpdatedAt = x.UpdatedAt
            };
        }).ToList();

        return new PagedResult<AggregatedInventory>
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
        var tenantContext = _context.GetService<POS.Domain.Interfaces.ITenantContext>();
        var query = _context.Inventories
            .IgnoreQueryFilters()
            .Where(i => i.VariantId == variantId);

        if (tenantContext.TenantId.HasValue)
        {
            query = query.Where(i => i.TenantId == tenantContext.TenantId.Value);
        }

        // If user is a Store Manager or store staff (not HQ / TenantAdmin / SuperAdmin), strictly filter to their own store
        var isGeneral = tenantContext.IsSuperAdmin || tenantContext.SystemRole == "TenantAdmin" || (tenantContext.SystemRole == "Manager" && !tenantContext.StoreId.HasValue);
        if (!isGeneral && tenantContext.StoreId.HasValue)
        {
            query = query.Where(i => i.StoreId == tenantContext.StoreId.Value);
        }

        return await query
            .Include(i => i.Store)
            .Include(i => i.Variant)
                .ThenInclude(v => v.Product)
            .OrderBy(i => i.Store.Name)
            .ToListAsync();
    }

    public async Task AddBatchAsync(InventoryBatch batch)
    {
        await _context.InventoryBatches.AddAsync(batch);
    }
}
