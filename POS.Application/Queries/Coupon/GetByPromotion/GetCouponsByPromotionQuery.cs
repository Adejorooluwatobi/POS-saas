using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Coupon.GetByPromotion;

public record GetCouponsByPromotionQuery(Guid PromotionId) : IRequest<IEnumerable<CouponDto>>;
