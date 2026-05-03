using MediatR;

namespace POS.Application.Commands.InventoryOrder.Dispatch;

public record DispatchInventoryOrderCommand(Guid Id) : IRequest;
