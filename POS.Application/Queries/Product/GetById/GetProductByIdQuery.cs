using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Product.GetById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
