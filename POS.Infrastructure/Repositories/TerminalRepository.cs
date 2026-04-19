using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class TerminalRepository : GenericRepository<Terminal>, ITerminalRepository
{
    public TerminalRepository(RetailOsDbContext context) : base(context) { }

    public async Task<IEnumerable<Terminal>> GetByStoreAsync(Guid storeId) =>
        await _dbSet.Where(t => t.StoreId == storeId).ToListAsync();
}
