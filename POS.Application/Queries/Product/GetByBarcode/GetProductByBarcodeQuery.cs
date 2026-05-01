using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Product.GetByBarcode;

public record GetProductByBarcodeQuery(string Barcode) : IRequest<ProductDto?>;
