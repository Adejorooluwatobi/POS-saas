using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IInventoryRepository : IGenericRepository<Inventory>
{
    Task<Inventory?> GetByVariantAndStoreAsync(Guid variantId, Guid storeId);
    Task<IEnumerable<Inventory>> GetLowStockAlertsAsync(Guid storeId);
    Task<IEnumerable<Inventory>> GetCrossStoreStockAsync(Guid variantId);
}
