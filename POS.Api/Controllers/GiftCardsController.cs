using MediatR;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.GiftCard.Issue;
using POS.Application.Commands.GiftCard.Redeem;
using POS.Application.DTOs;
using POS.Application.Queries.GiftCard.GetById;
using POS.Application.Queries.GiftCard.GetPaged;

using Microsoft.AspNetCore.Authorization;

using POS.Application.Commands.GiftCard.Update;
using POS.Application.Commands.GiftCard.Delete;

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

    /// <summary>Issues a new gift card for the current tenant.</summary>
    [HttpPost("issue")]
    public async Task<IActionResult> Issue([FromBody] IssueGiftCardDto dto)
    {
        var result = await _mediator.Send(new IssueGiftCardCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGiftCardDto dto)
    {
        await _mediator.Send(new UpdateGiftCardCommand(id, dto));
        return NoContent();
    }

    /// <summary>Redeems an amount from a gift card by card number.</summary>
    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemGiftCardDto dto)
    {
        var result = await _mediator.Send(new RedeemGiftCardCommand(dto));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteGiftCardCommand(id));
        return NoContent();
    }
}
