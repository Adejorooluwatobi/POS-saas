using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Product.GetPaged;

public record GetProductsPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<ProductDto>>;
