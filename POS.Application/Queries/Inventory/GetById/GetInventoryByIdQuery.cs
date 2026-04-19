using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Inventory.GetById;

public record GetInventoryByIdQuery(Guid Id) : IRequest<InventoryDto?>;
