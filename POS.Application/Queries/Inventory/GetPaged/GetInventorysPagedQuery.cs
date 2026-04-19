using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Inventory.GetPaged;

public record GetInventorysPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<InventoryDto>>;
