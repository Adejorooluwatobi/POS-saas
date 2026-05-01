using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Product.GetByBarcode;

public class GetProductByBarcodeQueryHandler : IRequestHandler<GetProductByBarcodeQuery, ProductDto?>
{
    private readonly IProductVariantRepository _variantRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductByBarcodeQueryHandler(
        IProductVariantRepository variantRepository,
        IProductRepository productRepository,
        IMapper mapper)
    {
        _variantRepository = variantRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
    {
        var variant = await _variantRepository.GetByBarcodeAsync(request.Barcode);
        if (variant == null) return null;

        var product = await _productRepository.GetByIdAsync(variant.ProductId);
        if (product == null) return null;

        var dto = _mapper.Map<ProductDto>(product);
        
        // Ensure the price and barcode from the scanned variant are reflected in the DTO if needed
        // (In a single-variant system, this is straightforward)
        return dto;
    }
}
