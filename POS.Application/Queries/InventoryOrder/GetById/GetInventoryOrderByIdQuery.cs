using MediatR;
using POS.Application.DTOs.InventoryOrder;

namespace POS.Application.Queries.InventoryOrder.GetById;

public record GetInventoryOrderByIdQuery(Guid Id) : IRequest<InventoryOrderDto>;
