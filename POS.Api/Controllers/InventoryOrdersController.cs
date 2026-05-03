using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.InventoryOrder.AcceptReferral;
using POS.Application.Commands.InventoryOrder.Approve;
using POS.Application.Commands.InventoryOrder.Cancel;
using POS.Application.Commands.InventoryOrder.Create;
using POS.Application.Commands.InventoryOrder.DeclineReferral;
using POS.Application.Commands.InventoryOrder.Dispatch;
using POS.Application.Commands.InventoryOrder.Dispute;
using POS.Application.Commands.InventoryOrder.Receive;
using POS.Application.Commands.InventoryOrder.Resolve;
using POS.Application.DTOs.InventoryOrder;
using POS.Application.Queries.InventoryOrder.GetById;
using POS.Application.Queries.InventoryOrder.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/inventory-orders")]
[Authorize(Policy = "StaffOnly")]
public class InventoryOrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public InventoryOrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetInventoryOrdersPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetInventoryOrderByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateInventoryOrderDto dto)
    {
        var result = await _mediator.Send(new CreateInventoryOrderCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(Guid id)
    {
        await _mediator.Send(new DispatchInventoryOrderCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> Receive(Guid id, [FromBody] ReceiveInventoryOrderDto dto)
    {
        await _mediator.Send(new ReceiveInventoryOrderCommand(id, dto));
        return NoContent();
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _mediator.Send(new ApproveInventoryOrderCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/dispute")]
    public async Task<IActionResult> Dispute(Guid id, [FromBody] DisputeOrderDto dto)
    {
        await _mediator.Send(new DisputeInventoryOrderCommand(id, dto));
        return NoContent();
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveDisputeDto dto)
    {
        await _mediator.Send(new ResolveDisputeCommand(id, dto));
        return NoContent();
    }

    [HttpPost("{id:guid}/accept-referral")]
    public async Task<IActionResult> AcceptReferral(Guid id)
    {
        await _mediator.Send(new AcceptReferralCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/decline-referral")]
    public async Task<IActionResult> DeclineReferral(Guid id, [FromQuery] string reason)
    {
        await _mediator.Send(new DeclineReferralCommand(id, reason));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _mediator.Send(new CancelInventoryOrderCommand(id));
        return NoContent();
    }
}
