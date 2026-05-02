using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IStoreProductOverrideRepository : IGenericRepository<StoreProductOverride>
{
    Task<StoreProductOverride?> GetByStoreAndProductAsync(Guid storeId, Guid productId);
    Task<List<StoreProductOverride>> GetByProductIdAsync(Guid productId);
}
