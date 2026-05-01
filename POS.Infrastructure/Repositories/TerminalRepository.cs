using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class TerminalRepository : GenericRepository<Terminal>, ITerminalRepository
{
    public TerminalRepository(RetailOsDbContext context) : base(context) { }

    public override async Task<Terminal?> GetByIdAsync(Guid id)
    {
        return await _dbSet.Include(t => t.Store).FirstOrDefaultAsync(t => t.Id == id);
    }

    public override async Task<POS.Domain.Common.PagedResult<Terminal>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _dbSet.Include(t => t.Store);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new POS.Domain.Common.PagedResult<Terminal>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<Terminal>> GetByStoreAsync(Guid storeId) =>
        await _dbSet.Where(t => t.StoreId == storeId).ToListAsync();

    public async Task<Terminal?> GetByPairingCodeAsync(string pairingCode) =>
        await _dbSet.FirstOrDefaultAsync(t => t.PairingCode == pairingCode);
}
