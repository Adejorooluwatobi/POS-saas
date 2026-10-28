using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Tenant.Create;
using POS.Application.Commands.Tenant.Delete;
using POS.Application.Commands.Tenant.Purge;
using POS.Application.Commands.Tenant.Restore;
using POS.Application.Commands.Tenant.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Tenant.ExportData;
using POS.Application.Queries.Tenant.GetBin;
using POS.Application.Queries.Tenant.GetById;
using POS.Application.Queries.Tenant.GetDetails;
using POS.Application.Queries.Tenant.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize(Policy = "AdminOnly")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TenantsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetTenantsPagedQuery(page, size)));

    [HttpGet("bin")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> GetBin([FromQuery] int page = 1, [FromQuery] int size = 50)
        => Ok(await _mediator.Send(new GetTenantsBinQuery(page, size)));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTenantByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/details")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> GetDetails(Guid id, [FromQuery] int? year = null, [FromQuery] int? month = null)
        => Ok(await _mediator.Send(new GetTenantDetailsQuery(id, year, month)));

    [HttpGet("{id:guid}/export")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> ExportData(Guid id)
    {
        var result = await _mediator.Send(new ExportTenantDataQuery(id));
        return File(result.CsvBytes, result.ContentType, result.FileName);
    }

    [HttpPost]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        var result = await _mediator.Send(new CreateTenantCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantDto dto)
    {
        await _mediator.Send(new UpdateTenantCommand(id, dto));
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] bool isActive)
    {
        await _mediator.Send(new POS.Application.Commands.Tenant.SetStatus.SetTenantStatusCommand(id, isActive));
        return NoContent();
    }

    /// <summary>Soft deletes tenant (moves to Recycle Bin)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTenantCommand(id));
        return NoContent();
    }

    /// <summary>Restores tenant from Recycle Bin</summary>
    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Restore(Guid id)
    {
        await _mediator.Send(new RestoreTenantCommand(id));
        return NoContent();
    }

    /// <summary>Permanently purges tenant and all its data from the database</summary>
    [HttpDelete("{id:guid}/purge")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Purge(Guid id)
    {
        await _mediator.Send(new PurgeTenantCommand(id));
        return NoContent();
    }
}
