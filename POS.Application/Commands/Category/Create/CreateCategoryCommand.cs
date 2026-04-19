using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Category.Create;

public record CreateCategoryCommand(CreateCategoryDto Dto) : IRequest<CategoryDto>;
