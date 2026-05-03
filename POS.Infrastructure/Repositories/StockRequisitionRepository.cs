using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class StockRequisitionRepository : GenericRepository<StockRequisition>, IStockRequisitionRepository
{
    public StockRequisitionRepository(RetailOsDbContext context) : base(context)
    {
    }
}
