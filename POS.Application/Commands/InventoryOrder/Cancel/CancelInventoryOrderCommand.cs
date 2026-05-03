using MediatR;

namespace POS.Application.Commands.InventoryOrder.Cancel;

public record CancelInventoryOrderCommand(Guid Id) : IRequest;
