using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IProductVariantRepository : IGenericRepository<ProductVariant>
{
    Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId);
}
