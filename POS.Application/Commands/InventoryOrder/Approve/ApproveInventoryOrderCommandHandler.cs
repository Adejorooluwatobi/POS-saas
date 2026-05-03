using MediatR;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.InventoryOrder.Approve;

public class ApproveInventoryOrderCommandHandler : IRequestHandler<ApproveInventoryOrderCommand>
{
    private readonly IInventoryOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IStockRequisitionRepository _requisitionRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public ApproveInventoryOrderCommandHandler(
        IInventoryOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IProductVariantRepository variantRepository,
        IStockRequisitionRepository requisitionRepository,
        IUnitOfWork uow,
        ITenantContext tenantContext)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _variantRepository = variantRepository;
        _requisitionRepository = requisitionRepository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(ApproveInventoryOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Status != InventoryOrderStatus.Received)
            throw new InvalidOperationException("Order must be received to be approved.");

        // Verification: only StoreManager can approve (based on user request)
        if (_tenantContext.SystemRole != "StoreManager" && !_tenantContext.IsSuperAdmin)
            throw new UnauthorizedAccessException("Only Store Managers can approve inventory deliveries.");

        if (_tenantContext.StoreId.HasValue && order.DestinationStoreId != _tenantContext.StoreId.Value)
            throw new UnauthorizedAccessException("You can only approve orders for your own store.");

        foreach (var item in order.Items)
        {
            var variant = await _variantRepository.GetByIdAsync(item.VariantId)
                ?? throw new KeyNotFoundException($"Variant {item.VariantId} not found.");

            var baseVariantId = variant.IsBaseUnit ? variant.Id : variant.BaseVariantId!.Value;
            var receivedQty = item.QuantityReceived ?? 0;
            var qtyInBaseUnits = (int)(receivedQty * variant.ConversionFactor);

            var inventory = await _inventoryRepository.GetByVariantAndStoreAsync(baseVariantId, order.DestinationStoreId);
            if (inventory == null)
            {
                // Create inventory record if it doesn't exist
                inventory = new Domain.Entities.Inventory
                {
                    TenantId = order.TenantId,
                    VariantId = baseVariantId,
                    StoreId = order.DestinationStoreId,
                    QuantityOnHand = qtyInBaseUnits
                };
                await _inventoryRepository.AddAsync(inventory);
            }
            else
            {
                inventory.QuantityOnHand += qtyInBaseUnits;
            }

            // --- Subtract from Source Store ---
            if (order.SourceStoreId.HasValue)
            {
                var sourceInventory = await _inventoryRepository.GetByVariantAndStoreAsync(baseVariantId, order.SourceStoreId.Value);
                if (sourceInventory != null)
                {
                    sourceInventory.QuantityOnHand -= qtyInBaseUnits;
                }
                // Note: If sourceInventory is null, it means the source store sent stock they didn't have recorded.
                // We allow it to continue but subtract nothing, or we could throw an error. 
                // In a professional POS, we'd usually allow negative stock if configured, or block it.
            }

            // Update Requisition fulfillment if linked
            if (order.StockRequisitionId.HasValue)
            {
                var requisition = await _requisitionRepository.GetByIdAsync(order.StockRequisitionId.Value);
                if (requisition != null)
                {
                    var reqItem = requisition.Items.FirstOrDefault(ri => ri.VariantId == item.VariantId);
                    if (reqItem != null)
                    {
                        reqItem.QuantityFulfilled += receivedQty;
                    }

                    // Check if all items in requisition are fulfilled
                    if (requisition.Items.All(ri => ri.QuantityFulfilled >= ri.QuantityRequested))
                    {
                        requisition.Status = RequisitionStatus.FullyFulfilled;
                    }
                    else
                    {
                        requisition.Status = RequisitionStatus.PartiallyFulfilled;
                    }
                }
            }
        }

        order.Status = InventoryOrderStatus.Approved;
        order.ApprovedByStaffId = _tenantContext.UserId;

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
