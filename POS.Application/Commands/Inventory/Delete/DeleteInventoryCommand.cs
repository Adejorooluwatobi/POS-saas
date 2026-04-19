using System;
using MediatR;

namespace POS.Application.Commands.Inventory.Delete;

public record DeleteInventoryCommand(Guid Id) : IRequest;
