using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.StockRequisition.Approve;
using POS.Application.Commands.StockRequisition.Cancel;
using POS.Application.Commands.StockRequisition.Create;
using POS.Application.Commands.StockRequisition.Reject;
using POS.Application.Commands.StockRequisition.Review;
using POS.Application.DTOs.StockRequisition;
using POS.Application.Queries.StockRequisition.GetById;
using POS.Application.Queries.StockRequisition.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/stock-requisitions")]
[Authorize(Policy = "StaffOnly")]
public class StockRequisitionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public StockRequisitionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetStockRequisitionsPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetStockRequisitionByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockRequisitionDto dto)
    {
        var result = await _mediator.Send(new CreateStockRequisitionCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/review")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Review(Guid id)
    {
        await _mediator.Send(new ReviewStockRequisitionCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRequisitionDto dto)
    {
        await _mediator.Send(new ApproveStockRequisitionCommand(id, dto));
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Reject(Guid id, [FromQuery] string reason)
    {
        await _mediator.Send(new RejectStockRequisitionCommand(id, reason));
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _mediator.Send(new CancelStockRequisitionCommand(id));
        return NoContent();
    }
}
