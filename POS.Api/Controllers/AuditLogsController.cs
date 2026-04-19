using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Queries.AuditLog.GetPaged;

namespace POS.Api.Controllers;

/// <summary>
/// Read-only audit trail — visible to tenant admins only.
/// No POST/PUT/DELETE: audit logs are written exclusively by the AuditInterceptor.
/// </summary>
[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = "AdminOnly")]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuditLogsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns a paged list of audit log entries for the given tenant.</summary>
    [HttpGet("{tenantId:guid}")]
    public async Task<IActionResult> GetPaged(
        Guid tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50)
        => Ok(await _mediator.Send(new GetAuditLogsPagedQuery(tenantId, page, size)));
}
