using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Inventory.GetCrossStoreStock;

public record GetCrossStoreStockQuery(Guid VariantId) : IRequest<IEnumerable<InventoryDto>>;
