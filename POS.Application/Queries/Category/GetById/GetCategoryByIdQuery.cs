using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Category.GetById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;
