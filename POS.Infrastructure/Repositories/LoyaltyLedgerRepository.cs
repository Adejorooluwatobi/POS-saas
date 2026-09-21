using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class LoyaltyLedgerRepository : GenericRepository<LoyaltyLedgerEntry>, ILoyaltyLedgerRepository
{
    public LoyaltyLedgerRepository(RetailOsDbContext context) : base(context) { }

    public async Task<IEnumerable<LoyaltyLedgerEntry>> GetByCustomerIdAsync(Guid customerId) =>
        await _dbSet
            .Include(l => l.Transaction)
            .Where(l => l.CustomerId == customerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
}
