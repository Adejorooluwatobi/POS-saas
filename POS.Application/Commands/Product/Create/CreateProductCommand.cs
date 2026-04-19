using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Product.Create;

public record CreateProductCommand(CreateProductDto Dto) : IRequest<ProductDto>;
