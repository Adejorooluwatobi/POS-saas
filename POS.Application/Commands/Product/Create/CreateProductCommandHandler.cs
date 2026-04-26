using System.Text.Json;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Product;

namespace POS.Application.Commands.Product.Create;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public CreateProductCommandHandler(
        IProductRepository repository, 
        IProductVariantRepository variantRepository,
        IUnitOfWork uow, 
        IMapper mapper, 
        ITenantContext tenantContext)
    {
        _repository = repository;
        _variantRepository = variantRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? request.Dto.TenantId;
        if (tenantId == null)
        {
            throw new UnauthorizedAccessException("Tenant context is missing.");
        }

        var entity = _mapper.Map<Entity>(request.Dto);
        entity.TenantId = tenantId.Value;

        await _repository.AddAsync(entity);
        
        // Create default variant
        var defaultVariant = new POS.Domain.Entities.ProductVariant
        {
            TenantId = tenantId.Value,
            ProductId = entity.Id,
            Sku = entity.MasterSku,
            Barcode = entity.MasterSku, // Default to SKU
            BasePrice = request.Dto.SellingPrice,
            CostPrice = request.Dto.CostPrice,
            WeightGrams = request.Dto.WeightGrams,
            UnitOfMeasure = request.Dto.UnitOfMeasure ?? "Each",
            IsActive = true,
            Attributes = JsonDocument.Parse("{}")
        };
        
        await _variantRepository.AddAsync(defaultVariant);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<ProductDto>(entity);
    }
}
