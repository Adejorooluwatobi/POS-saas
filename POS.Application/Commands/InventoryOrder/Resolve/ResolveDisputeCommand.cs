using MediatR;
using POS.Application.DTOs.InventoryOrder;

namespace POS.Application.Commands.InventoryOrder.Resolve;

public record ResolveDisputeCommand(Guid Id, ResolveDisputeDto Dto) : IRequest;
