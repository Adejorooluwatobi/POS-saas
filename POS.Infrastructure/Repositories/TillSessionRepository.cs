using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class TillSessionRepository : GenericRepository<TillSession>, ITillSessionRepository
{
    public TillSessionRepository(RetailOsDbContext context) : base(context)
    {
    }
}
