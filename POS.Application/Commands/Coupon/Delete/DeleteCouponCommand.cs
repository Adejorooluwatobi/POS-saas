using MediatR;

namespace POS.Application.Commands.Coupon.Delete;

// Soft-deactivates the coupon — no hard delete since it may be referenced in transactions.
public record DeleteCouponCommand(Guid Id) : IRequest;
