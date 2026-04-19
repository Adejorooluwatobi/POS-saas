using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Tenant.Create;
using POS.Application.Commands.Tenant.Delete;
using POS.Application.Commands.Tenant.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Tenant.GetById;
using POS.Application.Queries.Tenant.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize(Policy = "SuperAdminOnly")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TenantsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetTenantsPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTenantByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        var result = await _mediator.Send(new CreateTenantCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantDto dto)
    {
        await _mediator.Send(new UpdateTenantCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTenantCommand(id));
        return NoContent();
    }
}
