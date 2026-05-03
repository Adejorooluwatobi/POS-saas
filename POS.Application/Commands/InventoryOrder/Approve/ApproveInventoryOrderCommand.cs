using MediatR;

namespace POS.Application.Commands.InventoryOrder.Approve;

public record ApproveInventoryOrderCommand(Guid Id) : IRequest;
