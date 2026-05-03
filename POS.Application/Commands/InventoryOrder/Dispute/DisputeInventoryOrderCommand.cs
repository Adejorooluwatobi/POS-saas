using MediatR;
using POS.Application.DTOs.InventoryOrder;

namespace POS.Application.Commands.InventoryOrder.Dispute;

public record DisputeInventoryOrderCommand(Guid Id, DisputeOrderDto Dto) : IRequest;
