using MediatR;
using POS.Application.DTOs.InventoryOrder;

namespace POS.Application.Commands.InventoryOrder.Receive;

public record ReceiveInventoryOrderCommand(Guid Id, ReceiveInventoryOrderDto Dto) : IRequest;
