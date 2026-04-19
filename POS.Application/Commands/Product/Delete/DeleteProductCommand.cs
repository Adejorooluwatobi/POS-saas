using System;
using MediatR;

namespace POS.Application.Commands.Product.Delete;

public record DeleteProductCommand(Guid Id) : IRequest;
