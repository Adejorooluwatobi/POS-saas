using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Staff.Create;
using POS.Application.Commands.Staff.Delete;
using POS.Application.Commands.Staff.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Staff.GetById;
using POS.Application.Queries.Staff.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;
    public StaffController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "StaffOnly")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetStaffsPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "StaffOnly")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetStaffByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/stats")]
    [Authorize(Policy = "StaffOnly")]
    public async Task<IActionResult> GetStats(Guid id, [FromQuery] int? year = null, [FromQuery] int? month = null)
        => Ok(await _mediator.Send(new POS.Application.Queries.Staff.GetStats.GetStaffStatsQuery(id, year, month)));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateStaffDto dto)
    {
        var result = await _mediator.Send(new CreateStaffCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffDto dto)
    {
        await _mediator.Send(new UpdateStaffCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteStaffCommand(id));
        return NoContent();
    }
}
