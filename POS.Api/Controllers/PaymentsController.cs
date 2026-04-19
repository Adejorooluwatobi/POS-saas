using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Payment.Create;
using POS.Application.DTOs;
using POS.Application.Queries.Payment.GetById;
using POS.Application.Queries.Payment.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Policy = "StaffOnly")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetPaymentsPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Records a payment against a transaction.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var result = await _mediator.Send(new CreatePaymentCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
