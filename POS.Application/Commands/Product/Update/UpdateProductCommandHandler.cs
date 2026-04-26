using System.Linq;
using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace POS.Application.Commands.Product.Update;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _repository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(
        IProductRepository repository, 
        IProductVariantRepository variantRepository,
        IUnitOfWork uow, 
        IMapper mapper)
    {
        _repository = repository;
        _variantRepository = variantRepository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found.");

        _mapper.Map(request.Dto, entity);
        _repository.Update(entity);

        // Update default variant
        // Note: In a real system we might have multiple variants. 
        // Here we assume the first one is the default to update based on the simple UI.
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

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
