using Microsoft.EntityFrameworkCore;
using POS.Domain.Common;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Infrastructure.Repositories;

public class StockRequisitionRepository : GenericRepository<StockRequisition>, IStockRequisitionRepository
{
    public StockRequisitionRepository(RetailOsDbContext context) : base(context)
    {
    }

    public override async Task<StockRequisition?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(r => r.RequestingStore)
            .Include(r => r.CreatedBy)
            .Include(r => r.ReviewedBy)
            .Include(r => r.Items)
                .ThenInclude(i => i.Variant)
            .Include(r => r.FulfillmentOrders)
                .ThenInclude(o => o.SourceStore)
            .Include(r => r.FulfillmentOrders)
                .ThenInclude(o => o.DestinationStore)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public override async Task<PagedResult<StockRequisition>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var count = await _dbSet.CountAsync();
        var items = await _dbSet
            .Include(r => r.RequestingStore)
            .Include(r => r.CreatedBy)
            .Include(r => r.ReviewedBy)
            .Include(r => r.Items)
                .ThenInclude(i => i.Variant)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<StockRequisition>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
