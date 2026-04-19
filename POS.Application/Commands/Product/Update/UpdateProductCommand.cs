using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Product.Update;

public record UpdateProductCommand(Guid Id, UpdateProductDto Dto) : IRequest;
