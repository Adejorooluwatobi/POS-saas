using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class GiftCardRepository : GenericRepository<GiftCard>, IGiftCardRepository
{
    public GiftCardRepository(RetailOsDbContext context) : base(context) { }

    public async Task<GiftCard?> GetByCardNumberAsync(Guid tenantId, string cardNumber) =>
        await _dbSet.FirstOrDefaultAsync(g => g.TenantId == tenantId && g.CardNumber == cardNumber);
}
