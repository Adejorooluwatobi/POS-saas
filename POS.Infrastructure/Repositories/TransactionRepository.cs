using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Common;

namespace POS.Infrastructure.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(RetailOsDbContext context) : base(context)
    {
    }

    public override async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.Items)
            .Include(t => t.Payments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public override async Task<PagedResult<Transaction>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Transactions
            .Include(t => t.Items)
            .Include(t => t.Payments);

        var count = await query.CountAsync();
        var items = await query.OrderByDescending(t => t.CreatedAt)
                               .Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PagedResult<Transaction>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
