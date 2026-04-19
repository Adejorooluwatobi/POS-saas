using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Inventory.Create;
using POS.Application.Commands.Inventory.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Inventory.GetById;
using POS.Application.Queries.Inventory.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Policy = "StaffOnly")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;
    public InventoryController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetInventorysPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetInventoryByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Seeds initial stock for a product variant at a store.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryDto dto)
    {
        var result = await _mediator.Send(new CreateInventoryCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Adjusts stock levels (restock, shrinkage write-off, etc.).</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryDto dto)
    {
        await _mediator.Send(new UpdateInventoryCommand(id, dto));
        return NoContent();
    }
}
