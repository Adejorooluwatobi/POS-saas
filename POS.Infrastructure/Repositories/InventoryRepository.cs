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
        var query = _context.Inventory
            .Include(i => i.ProductVariant);

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
}
