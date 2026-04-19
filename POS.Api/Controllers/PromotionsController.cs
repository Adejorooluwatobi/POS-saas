using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Promotion.Create;
using POS.Application.Commands.Promotion.Delete;
using POS.Application.Commands.Promotion.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Promotion.GetById;
using POS.Application.Queries.Promotion.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/promotions")]
[Authorize(Policy = "StaffOnly")]
public class PromotionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PromotionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetPromotionsPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPromotionByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromotionDto dto)
    {
        var result = await _mediator.Send(new CreatePromotionCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromotionDto dto)
    {
        await _mediator.Send(new UpdatePromotionCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeletePromotionCommand(id));
        return NoContent();
    }
}
