using AutoMapper;
using MediatR;
using POS.Application.DTOs.InventoryOrder;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.InventoryOrder.Create;

public class CreateInventoryOrderCommandHandler : IRequestHandler<CreateInventoryOrderCommand, InventoryOrderDto>
{
    private readonly IInventoryOrderRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public CreateInventoryOrderCommandHandler(
        IInventoryOrderRepository repository, 
        IUnitOfWork uow, 
        IMapper mapper, 
        ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<InventoryOrderDto> Handle(CreateInventoryOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context missing.");
        var staffId = _tenantContext.UserId ?? throw new UnauthorizedAccessException("User context missing.");

        var orderNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var entity = new Domain.Entities.InventoryOrder
        {
            TenantId = tenantId,
            OrderNumber = orderNumber,
            Type = request.Dto.Type,
            Status = InventoryOrderStatus.Draft,
            SourceStoreId = request.Dto.SourceStoreId,
            DestinationStoreId = request.Dto.DestinationStoreId,
            CreatedByStaffId = staffId,
            Notes = request.Dto.Notes,
            StockRequisitionId = request.Dto.StockRequisitionId,
            IsReferredTransfer = request.Dto.Type == InventoryOrderType.StoreToStore && _tenantContext.SystemRole != "StoreManager",
            Items = request.Dto.Items.Select(i => new InventoryOrderItem
            {
                InventoryOrderId = Guid.Empty, // EF will fill this, but C# 11 requires it set if marked 'required'
                VariantId = i.VariantId,
                QuantityOrdered = i.QuantityOrdered
            }).ToList()
        };

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        // Load navigations for mapping if needed, or rely on mapper configuration
        return _mapper.Map<InventoryOrderDto>(entity);
    }
}
