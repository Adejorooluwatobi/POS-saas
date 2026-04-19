using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class StoreRepository : GenericRepository<Store>, IStoreRepository
{
    public StoreRepository(RetailOsDbContext context) : base(context)
    {
    }
}
