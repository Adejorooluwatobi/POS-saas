using POS.Domain.Common;
using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IInventoryRepository : IGenericRepository<Inventory>
{
    Task<PagedResult<AggregatedInventory>> GetAggregatedPagedAsync(int pageNumber, int pageSize);
    Task<Inventory?> GetByVariantAndStoreAsync(Guid variantId, Guid storeId);
    Task<IEnumerable<Inventory>> GetLowStockAlertsAsync(Guid storeId);
    Task<IEnumerable<Inventory>> GetCrossStoreStockAsync(Guid variantId);
    Task AddBatchAsync(InventoryBatch batch);
}
