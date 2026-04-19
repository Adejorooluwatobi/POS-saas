using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.TillSession.Create;
using POS.Application.Commands.TillSession.Update;
using POS.Application.DTOs;
using POS.Application.Queries.TillSession.GetById;
using POS.Application.Queries.TillSession.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/till-sessions")]
[Authorize(Policy = "StaffOnly")]
public class TillSessionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TillSessionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetTillSessionsPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTillSessionByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Opens a new till session (cashier clocks in at a terminal).</summary>
    [HttpPost]
    public async Task<IActionResult> Open([FromBody] CreateTillSessionDto dto)
    {
        var result = await _mediator.Send(new CreateTillSessionCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Closes or updates a till session (reconciliation, closing float).</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTillSessionDto dto)
    {
        await _mediator.Send(new UpdateTillSessionCommand(id, dto));
        return NoContent();
    }
}
