using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Store.GetPaged;

public record GetStoresPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<StoreDto>>;
