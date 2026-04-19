using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Store.Create;
using POS.Application.Commands.Store.Delete;
using POS.Application.Commands.Store.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Store.GetById;
using POS.Application.Queries.Store.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/stores")]
[Authorize(Policy = "AdminOnly")]
public class StoresController : ControllerBase
{
    private readonly IMediator _mediator;
    public StoresController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetStoresPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetStoreByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStoreDto dto)
    {
        var result = await _mediator.Send(new CreateStoreCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreDto dto)
    {
        await _mediator.Send(new UpdateStoreCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteStoreCommand(id));
        return NoContent();
    }
}
