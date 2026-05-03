using MediatR;
using POS.Application.DTOs.InventoryOrder;
using POS.Domain.Common;

namespace POS.Application.Queries.InventoryOrder.GetPaged;

public record GetInventoryOrdersPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<InventoryOrderDto>>;
