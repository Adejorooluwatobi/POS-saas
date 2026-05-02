using System.Linq;
using System.Text.Json;
using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Product.Update;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _repository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IProductBarcodeRepository _barcodeRepository;
    private readonly IStoreProductOverrideRepository _overrideRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public UpdateProductCommandHandler(
        IProductRepository repository, 
        IProductVariantRepository variantRepository,
        IProductBarcodeRepository barcodeRepository,
        IStoreProductOverrideRepository overrideRepository,
        IUnitOfWork uow, 
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _variantRepository = variantRepository;
        _barcodeRepository = barcodeRepository;
        _overrideRepository = overrideRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found.");

        // 1. Update Core Product (Global)
        _mapper.Map(request.Dto, entity);
        _repository.Update(entity);

        var variants = await _variantRepository.GetByProductIdAsync(request.Id);
        var defaultVariant = variants.FirstOrDefault();

        // 2. Handle Store-Specific Overrides (If TargetStoreIds provided)
        if (request.Dto.TargetStoreIds != null && request.Dto.TargetStoreIds.Any())
        {
            foreach (var storeId in request.Dto.TargetStoreIds)
            {
                var @override = await _overrideRepository.GetByStoreAndProductAsync(storeId, entity.Id);
                if (@override == null)
                {
                    @override = new POS.Domain.Entities.StoreProductOverride
                    {
                        TenantId = entity.TenantId,
                        StoreId = storeId,
                        ProductId = entity.Id,
                        Price = request.Dto.SellingPrice,
                        IsActive = request.Dto.IsActive
                    };
                    await _overrideRepository.AddAsync(@override);
                }
                else
                {
                    @override.Price = request.Dto.SellingPrice;
                    @override.IsActive = request.Dto.IsActive;
                    _overrideRepository.Update(@override);
                }
            }
        }
        else
        {
            // If no targeted stores, update the default variant (Global Price)
            if (defaultVariant != null)
            {
                defaultVariant.BasePrice = request.Dto.SellingPrice;
                defaultVariant.CostPrice = request.Dto.CostPrice;
                defaultVariant.WeightGrams = request.Dto.WeightGrams;
                defaultVariant.UnitOfMeasure = request.Dto.UnitOfMeasure ?? "Each";
                _variantRepository.Update(defaultVariant);
            }
        }

        // 3. Handle Multi-Barcoding (for the default variant)
        if (defaultVariant != null && request.Dto.Barcodes != null)
        {
            var existingBarcodes = await _barcodeRepository.GetByVariantIdAsync(defaultVariant.Id);
            
            // Remove barcodes not in the new list
            foreach (var eb in existingBarcodes)
            {
                if (!request.Dto.Barcodes.Contains(eb.BarcodeValue))
                    _barcodeRepository.Delete(eb);
            }

            // Add new barcodes
            foreach (var nb in request.Dto.Barcodes)
            {
                if (!existingBarcodes.Any(eb => eb.BarcodeValue == nb))
                {
                    await _barcodeRepository.AddAsync(new POS.Domain.Entities.ProductBarcode
                    {
                        TenantId = entity.TenantId,
                        VariantId = defaultVariant.Id,
                        StoreId = _tenantContext.StoreId,
                        BarcodeValue = nb
                    });
                }
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
