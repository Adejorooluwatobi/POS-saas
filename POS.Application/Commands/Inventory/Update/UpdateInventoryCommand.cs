using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Inventory.Update;

public record UpdateInventoryCommand(Guid Id, UpdateInventoryDto Dto) : IRequest;
