using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Coupon.Create;

public record CreateCouponCommand(Guid PromotionId, CreateCouponDto Dto) : IRequest<CouponDto>;
