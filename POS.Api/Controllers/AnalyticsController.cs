using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Queries.Analytics;
using System;
using System.Threading.Tasks;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Policy = "AdminOnly")] // Assuming only managers/admins should see analytics
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] Guid? storeId, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate, [FromQuery] int limit = 10)
    {
        var query = new GetTopSellingProductsQuery
        {
            StoreId = storeId,
            StartDate = startDate,
            EndDate = endDate,
            Limit = limit
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("busiest-hours")]
    public async Task<IActionResult> GetBusiestHours([FromQuery] Guid? storeId, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var query = new GetBusiestHoursQuery
        {
            StoreId = storeId,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("profit-margin")]
    public async Task<IActionResult> GetProfitMarginReport([FromQuery] Guid? storeId, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var query = new GetProfitMarginReportQuery
        {
            StoreId = storeId,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
