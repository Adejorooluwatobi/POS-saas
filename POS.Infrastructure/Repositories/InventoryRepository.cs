using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
{
    public InventoryRepository(RetailOsDbContext context) : base(context)
    {
    }
}
