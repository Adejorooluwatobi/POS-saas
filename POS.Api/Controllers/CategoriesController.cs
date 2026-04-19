using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Category.Create;
using POS.Application.Commands.Category.Delete;
using POS.Application.Commands.Category.Update;
using POS.Application.DTOs;
using POS.Application.Queries.Category.GetById;
using POS.Application.Queries.Category.GetPaged;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Policy = "StaffOnly")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _mediator.Send(new GetCategorysPagedQuery(page, size)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        await _mediator.Send(new UpdateCategoryCommand(id, dto));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        return NoContent();
    }
}
