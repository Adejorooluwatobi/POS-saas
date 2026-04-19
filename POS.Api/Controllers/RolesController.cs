using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Role.Create;
using POS.Application.Commands.Role.Delete;
using POS.Application.Commands.Role.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Role.GetById;
using POS.Application.Queries.Role.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Policy = "AdminOnly")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;
    public RolesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetRolesPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRoleByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var result = await _mediator.Send(new CreateRoleCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        await _mediator.Send(new UpdateRoleCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteRoleCommand(id));
        return NoContent();
    }
}
