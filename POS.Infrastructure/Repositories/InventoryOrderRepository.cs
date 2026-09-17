using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using POS.Domain.Common;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Infrastructure.Repositories;

public class InventoryOrderRepository : GenericRepository<InventoryOrder>, IInventoryOrderRepository
{
    public InventoryOrderRepository(RetailOsDbContext context) : base(context)
    {
    }

    public override async Task<InventoryOrder?> GetByIdAsync(Guid id)
    {
        var tenantContext = _context.GetService<POS.Domain.Interfaces.ITenantContext>();
        var query = _dbSet.IgnoreQueryFilters();
        
        if (tenantContext.TenantId.HasValue)
            query = query.Where(o => o.TenantId == tenantContext.TenantId.Value);
            
        if (tenantContext.StoreId.HasValue)
            query = query.Where(o => o.DestinationStoreId == tenantContext.StoreId.Value || o.SourceStoreId == tenantContext.StoreId.Value);

        return await query
            .Include(o => o.SourceStore)
            .Include(o => o.DestinationStore)
            .Include(o => o.CreatedBy)
            .Include(o => o.ReceivedBy)
            .Include(o => o.ApprovedBy)
            .Include(o => o.ResolvedBy)
            .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public override async Task<PagedResult<InventoryOrder>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var tenantContext = _context.GetService<POS.Domain.Interfaces.ITenantContext>();
        var query = _dbSet.IgnoreQueryFilters();
        
        if (tenantContext.TenantId.HasValue)
            query = query.Where(o => o.TenantId == tenantContext.TenantId.Value);
            
        if (tenantContext.StoreId.HasValue)
            query = query.Where(o => o.DestinationStoreId == tenantContext.StoreId.Value || o.SourceStoreId == tenantContext.StoreId.Value);

        var count = await query.CountAsync();
        var items = await query
            .Include(o => o.SourceStore)
            .Include(o => o.DestinationStore)
            .Include(o => o.CreatedBy)
            .Include(o => o.ReceivedBy)
            .Include(o => o.ApprovedBy)
            .Include(o => o.ResolvedBy)
            .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
                    .ThenInclude(v => v.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<InventoryOrder>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
