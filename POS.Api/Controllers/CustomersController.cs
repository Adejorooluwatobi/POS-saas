using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Customer.Create;
using POS.Application.Commands.Customer.Delete;
using POS.Application.Commands.Customer.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Customer.GetById;
using POS.Application.Queries.Customer.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Policy = "StaffOnly")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    public CustomersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetCustomersPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var result = await _mediator.Send(new CreateCustomerCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        await _mediator.Send(new UpdateCustomerCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCustomerCommand(id));
        return NoContent();
    }
}
