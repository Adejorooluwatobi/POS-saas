using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.Commands.Loyalty.UpdateLoyaltySettings;
using POS.Application.DTOs;
using POS.Application.Queries.Loyalty.GetLoyaltySettings;
using POS.Infrastructure.Data;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/loyalty")]
[Authorize]
public class LoyaltyController : ControllerBase
{
    private readonly RetailOsDbContext _context;
    private readonly IMediator _mediator;

    public LoyaltyController(RetailOsDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    [HttpGet("ledger")]
    public async Task<IActionResult> GetGlobalLedger([FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        var query = _context.LoyaltyLedger
            .Include(l => l.Customer)
            .Include(l => l.Transaction)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size)
            .Select(l => new
            {
                l.Id,
                l.CustomerId,
                CustomerName = l.Customer != null ? l.Customer.FirstName + " " + l.Customer.LastName : "Unknown Customer",
                CustomerEmail = l.Customer != null ? l.Customer.Email : null,
                CustomerPhone = l.Customer != null ? l.Customer.Phone : null,
                CustomerLoyaltyCardNo = l.Customer != null ? l.Customer.LoyaltyCardNo : null,
                l.TransactionId,
                TransactionTotal = l.Transaction != null ? (decimal?)l.Transaction.GrandTotal : null,
                l.Delta,
                l.Reason,
                l.BalanceAfter,
                l.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            items,
            totalCount = total,
            pageNumber = page,
            pageSize = size
        });
    }

    [HttpGet("ledger/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerLedger(Guid customerId)
    {
        var items = await _context.LoyaltyLedger
            .Include(l => l.Transaction)
            .Where(l => l.CustomerId == customerId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new
            {
                l.Id,
                l.CustomerId,
                l.TransactionId,
                TransactionTotal = l.Transaction != null ? (decimal?)l.Transaction.GrandTotal : null,
                l.Delta,
                l.Reason,
                l.BalanceAfter,
                l.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("entry/{id:guid}")]
    public async Task<IActionResult> GetEntryById(Guid id)
    {
        var entry = await _context.LoyaltyLedger
            .Include(l => l.Customer)
            .Include(l => l.Transaction)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (entry == null) return NotFound();

        return Ok(new
        {
            entry.Id,
            entry.CustomerId,
            CustomerName = entry.Customer != null ? entry.Customer.FirstName + " " + entry.Customer.LastName : "Unknown",
            CustomerEmail = entry.Customer != null ? entry.Customer.Email : null,
            CustomerPhone = entry.Customer != null ? entry.Customer.Phone : null,
            CustomerLoyaltyCardNo = entry.Customer != null ? entry.Customer.LoyaltyCardNo : null,
            entry.TransactionId,
            TransactionTotal = entry.Transaction != null ? (decimal?)entry.Transaction.GrandTotal : null,
            entry.Delta,
            entry.Reason,
            entry.BalanceAfter,
            entry.CreatedAt
        });
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetLoyaltySettings([FromQuery] Guid? tenantId = null)
    {
        var settings = await _mediator.Send(new GetLoyaltySettingsQuery(tenantId));
        return Ok(settings);
    }

    [HttpPut("settings")]
    [Authorize(Policy = "TenantAdminOnly")]
    public async Task<IActionResult> UpdateLoyaltySettings([FromBody] LoyaltySettingsDto dto)
    {
        var settings = await _mediator.Send(new UpdateLoyaltySettingsCommand(dto));
        return Ok(settings);
    }
}
