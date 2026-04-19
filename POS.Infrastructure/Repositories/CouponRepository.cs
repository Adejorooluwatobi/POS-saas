using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
{
    public CouponRepository(RetailOsDbContext context) : base(context) { }

    public async Task<IEnumerable<Coupon>> GetByPromotionAsync(Guid promotionId) =>
        await _dbSet.Where(c => c.PromotionId == promotionId).ToListAsync();

    public async Task<Coupon?> GetByCodeAsync(string code) =>
        await _dbSet.FirstOrDefaultAsync(c => c.Code == code);
}
