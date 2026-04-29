using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(RetailOsDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId)
    {
        return await _context.ProductVariants
            .Where(v => v.ProductId == productId)
            .ToListAsync();
    }
}
