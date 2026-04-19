using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.TenantSubscription.Cancel;
using POS.Application.Commands.TenantSubscription.Update;
using POS.Application.DTOs;
using POS.Application.Queries.TenantSubscription.GetByTenant;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize(Policy = "AdminOnly")]
public class TenantSubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TenantSubscriptionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns the active subscription for a tenant.</summary>
    [HttpGet("{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId)
    {
        var result = await _mediator.Send(new GetSubscriptionByTenantQuery(tenantId));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Upgrades or downgrades the subscription plan / billing cycle.</summary>
    [HttpPut("{tenantId:guid}")]
    public async Task<IActionResult> Update(Guid tenantId, [FromBody] UpdateSubscriptionDto dto)
    {
        var result = await _mediator.Send(new UpdateSubscriptionCommand(tenantId, dto));
        return Ok(result);
    }

    /// <summary>Cancels the subscription for a tenant.</summary>
    [HttpDelete("{tenantId:guid}")]
    public async Task<IActionResult> Cancel(Guid tenantId)
    {
        await _mediator.Send(new CancelSubscriptionCommand(tenantId));
        return NoContent();
    }
}
