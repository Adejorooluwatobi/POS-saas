using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
{
    public PromotionRepository(RetailOsDbContext context) : base(context)
    {
    }
}
