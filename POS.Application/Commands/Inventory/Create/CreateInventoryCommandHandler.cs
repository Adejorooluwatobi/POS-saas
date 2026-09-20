using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Inventory;

namespace POS.Application.Commands.Inventory.Create;

public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand, InventoryDto>
{
    private readonly IInventoryRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public CreateInventoryCommandHandler(IInventoryRepository repository, IUnitOfWork uow, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<InventoryDto> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("Super admins are not allowed to create inventory records.");
        }

        var entity = _mapper.Map<Entity>(request.Dto);
        if (_tenantContext.TenantId.HasValue)
        {
            entity.TenantId = _tenantContext.TenantId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Dto.BatchNumber) || request.Dto.ExpiryDate.HasValue)
        {
            var batch = new POS.Domain.Entities.InventoryBatch
            {
                TenantId = entity.TenantId,
                InventoryId = entity.Id,
                VariantId = entity.VariantId,
                StoreId = entity.StoreId,
                BatchNumber = !string.IsNullOrWhiteSpace(request.Dto.BatchNumber) 
                    ? request.Dto.BatchNumber 
                    : $"LOT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                ProductionDate = request.Dto.ProductionDate,
                ExpiryDate = request.Dto.ExpiryDate,
                QuantityOnHand = entity.QuantityOnHand,
                QuantityReserved = 0,
                ExpiryAlertPercentage = request.Dto.ExpiryAlertPercentage ?? 30,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            entity.Batches.Add(batch);
        }
        
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<InventoryDto>(entity);
    }
}
