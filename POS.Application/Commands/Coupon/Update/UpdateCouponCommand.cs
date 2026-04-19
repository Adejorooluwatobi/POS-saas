using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Coupon.Update;

public record UpdateCouponCommand(Guid Id, UpdateCouponDto Dto) : IRequest;
