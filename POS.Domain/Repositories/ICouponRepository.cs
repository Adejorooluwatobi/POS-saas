using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface ICouponRepository : IGenericRepository<Coupon>
{
    Task<IEnumerable<Coupon>> GetByPromotionAsync(Guid promotionId);
    Task<Coupon?> GetByCodeAsync(string code);
}
