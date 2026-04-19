using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Transaction.Create;
using POS.Application.Commands.Transaction.Delete;
using POS.Application.Commands.Transaction.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Transaction.GetById;
using POS.Application.Queries.Transaction.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize(Policy = "StaffOnly")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TransactionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetTransactionsPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTransactionByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        var result = await _mediator.Send(new CreateTransactionCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionDto dto)
    {
        await _mediator.Send(new UpdateTransactionCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTransactionCommand(id));
        return NoContent();
    }
}
