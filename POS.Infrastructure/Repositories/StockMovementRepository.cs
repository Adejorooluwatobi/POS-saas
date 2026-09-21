using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class StockMovementRepository : GenericRepository<StockMovement>, IStockMovementRepository
{
    public StockMovementRepository(RetailOsDbContext context) : base(context)
    {
    }
}
