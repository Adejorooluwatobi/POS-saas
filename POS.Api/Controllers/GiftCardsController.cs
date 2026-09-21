using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.GiftCard.Delete;
using POS.Application.Commands.GiftCard.Issue;
using POS.Application.Commands.GiftCard.LinkCustomer;
using POS.Application.Commands.GiftCard.Recharge;
using POS.Application.Commands.GiftCard.Redeem;
using POS.Application.Commands.GiftCard.ReplaceLost;
using POS.Application.Commands.GiftCard.SetStatus;
using POS.Application.Commands.GiftCard.Transfer;
using POS.Application.Commands.GiftCard.Update;
using POS.Application.DTOs;
using POS.Application.Queries.GiftCard.GetById;
using POS.Application.Queries.GiftCard.GetByNumber;
using POS.Application.Queries.GiftCard.GetPaged;
using POS.Application.Queries.GiftCard.GetTransactions;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/gift-cards")]
[Authorize(Policy = "StaffOnly")]
public class GiftCardsController : ControllerBase
{
    private readonly IMediator _mediator;
    public GiftCardsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetGiftCardsPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetGiftCardByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-number/{cardNumber}")]
    public async Task<IActionResult> GetByNumber(string cardNumber)
    {
        var result = await _mediator.Send(new GetGiftCardByNumberQuery(cardNumber));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Issues a new gift card for the current tenant.</summary>
    [HttpPost("issue")]
    [Authorize(Policy = "TenantStaffOnly")]
    public async Task<IActionResult> Issue([FromBody] IssueGiftCardDto dto)
    {
        var result = await _mediator.Send(new IssueGiftCardCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Recharges an existing gift card via cash, bank transfer, or mobile gateway reference.</summary>
    [HttpPost("recharge")]
    [Authorize(Policy = "TenantStaffOnly")]
    public async Task<IActionResult> Recharge([FromBody] RechargeGiftCardDto dto)
    {
        var result = await _mediator.Send(new RechargeGiftCardCommand(dto));
        return Ok(result);
    }

    /// <summary>Transfers funds between two cards.</summary>
    [HttpPost("transfer")]
    [Authorize(Policy = "TenantStaffOnly")]
    public async Task<IActionResult> Transfer([FromBody] TransferCardBalanceDto dto)
    {
        var result = await _mediator.Send(new TransferCardBalanceCommand(dto));
        return Ok(result);
    }

    /// <summary>Links a customer to a gift card.</summary>
    [HttpPost("{id:guid}/link-customer")]
    [Authorize(Policy = "TenantStaffOnly")]
    public async Task<IActionResult> LinkCustomer(Guid id, [FromBody] LinkGiftCardCustomerDto dto)
    {
        var result = await _mediator.Send(new LinkGiftCardCustomerCommand(id, dto.CustomerId));
        return Ok(result);
    }

    /// <summary>Unlinks customer from a gift card.</summary>
    [HttpPost("{id:guid}/unlink-customer")]
    [Authorize(Policy = "TenantStaffOnly")]
    public async Task<IActionResult> UnlinkCustomer(Guid id)
    {
        var result = await _mediator.Send(new UnlinkGiftCardCustomerCommand(id));
        return Ok(result);
    }

    /// <summary>Gets financial audit transactions for a card.</summary>
    [HttpGet("{id:guid}/transactions")]
    [Authorize(Policy = "TenantStaffOnly")]
    public async Task<IActionResult> GetTransactions(Guid id)
    {
        var result = await _mediator.Send(new GetGiftCardTransactionsQuery(id));
        return Ok(result);
    }

    /// <summary>Activates or deactivates a gift card.</summary>
    [HttpPost("{id:guid}/set-status")]
    [Authorize(Policy = "TenantStaffOnly")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetGiftCardStatusDto dto)
    {
        var result = await _mediator.Send(new SetGiftCardStatusCommand(id, dto));
        return Ok(result);
    }

    /// <summary>Reports a lost/misplaced card, deactivates it, creates a replacement, and migrates balance.</summary>
    [HttpPost("replace-lost")]
    [Authorize(Policy = "TenantStaffOnly")]
    public async Task<IActionResult> ReplaceLost([FromBody] ReplaceLostGiftCardDto dto)
    {
        var result = await _mediator.Send(new ReplaceLostCardCommand(dto));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "TenantAdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGiftCardDto dto)
    {
        await _mediator.Send(new UpdateGiftCardCommand(id, dto));
        return NoContent();
    }

    /// <summary>Redeems an amount from a gift card by card number.</summary>
    [HttpPost("redeem")]
    [Authorize(Policy = "TenantStaffOnly")]
    public async Task<IActionResult> Redeem([FromBody] RedeemGiftCardDto dto)
    {
        var result = await _mediator.Send(new RedeemGiftCardCommand(dto));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "TenantAdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteGiftCardCommand(id));
        return NoContent();
    }
}
