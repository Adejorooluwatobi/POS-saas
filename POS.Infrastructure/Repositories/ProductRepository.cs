using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace POS.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(RetailOsDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Variants)
            .ToListAsync();
    }

    public override async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<POS.Domain.Common.PagedResult<Product>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Products.Include(p => p.Variants);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new POS.Domain.Common.PagedResult<Product>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
