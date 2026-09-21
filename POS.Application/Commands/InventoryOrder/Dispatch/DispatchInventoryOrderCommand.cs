using MediatR;
using POS.Application.DTOs.InventoryOrder;

namespace POS.Application.Commands.InventoryOrder.Dispatch;

public record DispatchInventoryOrderCommand(Guid Id, DispatchInventoryOrderDto? Dto = null) : IRequest;
