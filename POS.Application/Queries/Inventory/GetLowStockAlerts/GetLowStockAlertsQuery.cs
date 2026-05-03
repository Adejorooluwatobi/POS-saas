using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Inventory.GetLowStockAlerts;

public record GetLowStockAlertsQuery(Guid StoreId) : IRequest<IEnumerable<InventoryDto>>;
