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
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public UpdateProductCommandHandler(
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

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found.");

        _mapper.Map(request.Dto, entity);
        _repository.Update(entity);

        // Update or Create default variant
        var variants = await _variantRepository.GetByProductIdAsync(request.Id);
        var defaultVariant = variants.FirstOrDefault();

        if (defaultVariant != null)
        {
            defaultVariant.BasePrice = request.Dto.SellingPrice;
            defaultVariant.CostPrice = request.Dto.CostPrice;
            defaultVariant.WeightGrams = request.Dto.WeightGrams;
            defaultVariant.UnitOfMeasure = request.Dto.UnitOfMeasure ?? "Each";
            _variantRepository.Update(defaultVariant);
        }
        else
        {
            // Create if missing (e.g. for old data)
            var newVariant = new POS.Domain.Entities.ProductVariant
            {
                TenantId = entity.TenantId,
                ProductId = entity.Id,
                Sku = entity.MasterSku,
                Barcode = entity.MasterSku,
                BasePrice = request.Dto.SellingPrice,
                CostPrice = request.Dto.CostPrice,
                WeightGrams = request.Dto.WeightGrams,
                UnitOfMeasure = request.Dto.UnitOfMeasure ?? "Each",
                IsActive = true,
                Attributes = JsonDocument.Parse("{}")
            };
            await _variantRepository.AddAsync(newVariant);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
