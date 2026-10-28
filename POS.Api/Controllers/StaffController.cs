using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Staff.Create;
using POS.Application.Commands.Staff.Delete;
using POS.Application.Commands.Staff.GeneratePassword;
using POS.Application.Commands.Staff.Purge;
using POS.Application.Commands.Staff.Restore;
using POS.Application.Commands.Staff.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Staff.GetBin;
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

    [HttpGet("bin")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetBin([FromQuery] int page = 1, [FromQuery] int size = 50)
        => Ok(await _mediator.Send(new GetStaffBinQuery(page, size)));

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
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffDto dto)
    {
        await _mediator.Send(new UpdateStaffCommand(id, dto));
        return NoContent();
    }

    /// <summary>Soft delete staff (move to staff bin)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteStaffCommand(id));
        return NoContent();
    }

    /// <summary>Restore staff from staff bin</summary>
    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Restore(Guid id)
    {
        await _mediator.Send(new RestoreStaffCommand(id));
        return NoContent();
    }

    /// <summary>Permanently purge staff record</summary>
    [HttpDelete("{id:guid}/purge")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Purge(Guid id)
    {
        await _mediator.Send(new PurgeStaffCommand(id));
        return NoContent();
    }

    /// <summary>Generate temporary credentials for staff enforcing hierarchical permissions</summary>
    [HttpPost("{id:guid}/generate-password")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GeneratePassword(Guid id)
    {
        var result = await _mediator.Send(new GenerateStaffPasswordCommand(id));
        return Ok(result);
    }
}
