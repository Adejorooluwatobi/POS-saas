using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Inventory.Create;

public record CreateInventoryCommand(CreateInventoryDto Dto) : IRequest<InventoryDto>;
