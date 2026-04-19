using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Category.GetPaged;

public record GetCategorysPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<CategoryDto>>;
