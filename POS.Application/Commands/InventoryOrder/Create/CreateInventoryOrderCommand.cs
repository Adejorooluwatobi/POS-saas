using MediatR;
using POS.Application.DTOs.InventoryOrder;

namespace POS.Application.Commands.InventoryOrder.Create;

public record CreateInventoryOrderCommand(CreateInventoryOrderDto Dto) : IRequest<InventoryOrderDto>;
