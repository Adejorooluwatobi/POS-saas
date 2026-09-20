using Microsoft.EntityFrameworkCore;
using POS.Domain.Common;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class GiftCardRepository : GenericRepository<GiftCard>, IGiftCardRepository
{
    public GiftCardRepository(RetailOsDbContext context) : base(context) { }

    public override async Task<GiftCard?> GetByIdAsync(Guid id) =>
        await GetByIdWithDetailsAsync(id);

    public async Task<GiftCard?> GetByCardNumberAsync(Guid tenantId, string cardNumber) =>
        await _dbSet.Include(g => g.Customer).Include(g => g.IssuingStore).FirstOrDefaultAsync(g => g.TenantId == tenantId && g.CardNumber == cardNumber);

    public async Task<GiftCard?> GetByIdWithDetailsAsync(Guid id) =>
        await _dbSet.Include(g => g.Customer).Include(g => g.IssuingStore).FirstOrDefaultAsync(g => g.Id == id);

    public override async Task<PagedResult<GiftCard>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var count = await _dbSet.CountAsync();
        var items = await _dbSet
            .Include(g => g.Customer)
            .Include(g => g.IssuingStore)
            .OrderByDescending(g => g.IssuedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<GiftCard>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
