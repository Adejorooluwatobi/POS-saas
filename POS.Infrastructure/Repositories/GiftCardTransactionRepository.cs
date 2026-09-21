using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class GiftCardTransactionRepository : GenericRepository<GiftCardTransaction>, IGiftCardTransactionRepository
{
    public GiftCardTransactionRepository(RetailOsDbContext context) : base(context) { }

    public async Task<List<GiftCardTransaction>> GetByCardIdAsync(Guid cardId) =>
        await _dbSet
            .Include(t => t.Staff)
            .Include(t => t.Store)
            .Where(t => t.GiftCardId == cardId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
}
