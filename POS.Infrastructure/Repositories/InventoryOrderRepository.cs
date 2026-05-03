using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class InventoryOrderRepository : GenericRepository<InventoryOrder>, IInventoryOrderRepository
{
    public InventoryOrderRepository(RetailOsDbContext context) : base(context)
    {
    }
}
