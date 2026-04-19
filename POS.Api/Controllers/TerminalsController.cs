using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Terminal.Create;
using POS.Application.Commands.Terminal.Delete;
using POS.Application.Commands.Terminal.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Terminal.GetById;
using POS.Application.Queries.Terminal.GetByStore;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/terminals")]
[Authorize(Policy = "StaffOnly")]
public class TerminalsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TerminalsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists all terminals belonging to a specific store.</summary>
    [HttpGet("by-store/{storeId:guid}")]
    public async Task<IActionResult> GetByStore(Guid storeId)
        => Ok(await _mediator.Send(new GetTerminalsByStoreQuery(storeId)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTerminalByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Registers a new terminal under a store.</summary>
    [HttpPost("{storeId:guid}")]
    public async Task<IActionResult> Create(Guid storeId, [FromBody] CreateTerminalDto dto)
    {
        var result = await _mediator.Send(new CreateTerminalCommand(storeId, dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Updates terminal label, IP address, or status.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTerminalDto dto)
    {
        await _mediator.Send(new UpdateTerminalCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTerminalCommand(id));
        return NoContent();
    }
}
