using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Coupon.Create;
using POS.Application.Commands.Coupon.Delete;
using POS.Application.Commands.Coupon.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Coupon.GetByPromotion;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/coupons")]
[Authorize(Policy = "StaffOnly")]
public class CouponsController : ControllerBase
{
    private readonly IMediator _mediator;
    public CouponsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists all coupons belonging to a specific promotion.</summary>
    [HttpGet("by-promotion/{promotionId:guid}")]
    public async Task<IActionResult> GetByPromotion(Guid promotionId)
        => Ok(await _mediator.Send(new GetCouponsByPromotionQuery(promotionId)));

    /// <summary>Creates a new coupon under a promotion.</summary>
    [HttpPost("{promotionId:guid}")]
    public async Task<IActionResult> Create(Guid promotionId, [FromBody] CreateCouponDto dto)
    {
        var result = await _mediator.Send(new CreateCouponCommand(promotionId, dto));
        return CreatedAtAction(nameof(GetByPromotion), new { promotionId = result.PromotionId }, result);
    }

    /// <summary>Updates coupon max-uses, active flag, or expiry.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponDto dto)
    {
        await _mediator.Send(new UpdateCouponCommand(id, dto));
        return NoContent();
    }

    /// <summary>Deactivates a coupon (soft delete — coupon may be referenced in past transactions).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _mediator.Send(new DeleteCouponCommand(id));
        return NoContent();
    }
}
