using MediatR;
using POS.Application.DTOs.InventoryOrder;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.InventoryOrder.Resolve;

public class ResolveDisputeCommandHandler : IRequestHandler<ResolveDisputeCommand>
{
    private readonly IInventoryOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IStockRequisitionRepository _requisitionRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    private readonly IEmailService _emailService;
    private readonly IStaffRepository _staffRepository;
    private readonly IStoreRepository _storeRepository;

    public ResolveDisputeCommandHandler(
        IInventoryOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IProductVariantRepository variantRepository,
        IStockRequisitionRepository requisitionRepository,
        IUnitOfWork uow,
        ITenantContext tenantContext,
        IEmailService emailService,
        IStaffRepository staffRepository,
        IStoreRepository storeRepository)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _variantRepository = variantRepository;
        _requisitionRepository = requisitionRepository;
        _uow = uow;
        _tenantContext = tenantContext;
        _emailService = emailService;
        _staffRepository = staffRepository;
        _storeRepository = storeRepository;
    }

    public async Task Handle(ResolveDisputeCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Status != InventoryOrderStatus.Disputed)
            throw new InvalidOperationException("Order must be in disputed status to be resolved.");

        // Verification: TenantAdmin or Source StoreManager can resolve
        var isGeneral = _tenantContext.SystemRole == "SuperAdmin" || _tenantContext.SystemRole == "TenantAdmin" || _tenantContext.SystemRole == "Manager";
        var isSourceManager = order.SourceStoreId.HasValue && _tenantContext.StoreId.HasValue && order.SourceStoreId.Value == _tenantContext.StoreId.Value && _tenantContext.SystemRole == "StoreManager";

        if (!isGeneral && !isSourceManager)
            throw new UnauthorizedAccessException("You don't have permission to resolve this dispute.");

        foreach (var itemDto in request.Dto.Items)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == itemDto.ItemId)
                ?? throw new KeyNotFoundException($"Item {itemDto.ItemId} not found in order.");

            var variant = await _variantRepository.GetByIdAsync(item.VariantId)
                ?? throw new KeyNotFoundException($"Variant {item.VariantId} not found.");

            var baseVariantId = variant.IsBaseUnit ? variant.Id : variant.BaseVariantId!.Value;
            var finalAgreedQty = itemDto.FinalAgreedQuantity;
            var qtyInBaseUnits = (int)(finalAgreedQty * variant.ConversionFactor);

            // 1. Update Destination Store stock
            var destInventory = await _inventoryRepository.GetByVariantAndStoreAsync(baseVariantId, order.DestinationStoreId);
            if (destInventory == null)
            {
                destInventory = new Domain.Entities.Inventory
                {
                    TenantId = order.TenantId,
                    VariantId = baseVariantId,
                    StoreId = order.DestinationStoreId,
                    QuantityOnHand = qtyInBaseUnits
                };
                await _inventoryRepository.AddAsync(destInventory);
            }
            else
            {
                destInventory.QuantityOnHand += qtyInBaseUnits;
            }

            // 2. If Store-to-Store transfer, return difference to Source Store
            if (order.SourceStoreId.HasValue)
            {
                var originalOrderedQty = item.QuantityOrdered;
                var shortage = originalOrderedQty - finalAgreedQty;
                if (shortage > 0)
                {
                    var shortageInBaseUnits = (int)(shortage * variant.ConversionFactor);
                    var sourceInventory = await _inventoryRepository.GetByVariantAndStoreAsync(baseVariantId, order.SourceStoreId.Value);
                    if (sourceInventory != null)
                    {
                        sourceInventory.QuantityOnHand += shortageInBaseUnits;
                    }
                }
            }

            // 3. Update Requisition fulfillment if linked
            if (order.StockRequisitionId.HasValue)
            {
                var requisition = await _requisitionRepository.GetByIdAsync(order.StockRequisitionId.Value);
                if (requisition != null)
                {
                    var reqItem = requisition.Items.FirstOrDefault(ri => ri.VariantId == item.VariantId);
                    if (reqItem != null)
                    {
                        reqItem.QuantityFulfilled += finalAgreedQty;
                    }

                    // Check if all items in requisition are fulfilled
                    if (requisition.Items.All(ri => ri.QuantityFulfilled >= ri.QuantityRequested))
                        requisition.Status = RequisitionStatus.FullyFulfilled;
                    else
                        requisition.Status = RequisitionStatus.PartiallyFulfilled;
                }
            }

            // Update item record
            item.QuantityReceived = finalAgreedQty;
        }

        order.Status = InventoryOrderStatus.Resolved;
        order.ResolvedByStaffId = _tenantContext.UserId;
        order.ResolvedAt = DateTimeOffset.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);

        // ── Trigger Email Notification ───────────────────────────────────
        try
        {
            var destinationStore = await _storeRepository.GetByIdAsync(order.DestinationStoreId);
            var managers = await _staffRepository.GetManagersByStoreAsync(order.DestinationStoreId);
            
            var itemsHtml = string.Join("", order.Items.Select(i => 
                $"<tr><td style='padding: 8px 0; border-bottom: 1px solid #F1F5F9;'>{i.Variant?.Sku ?? "Item"}</td>" +
                $"<td style='padding: 8px 0; border-bottom: 1px solid #F1F5F9;'>Final Qty: {i.QuantityReceived}</td></tr>"));

            foreach (var manager in managers)
            {
                await _emailService.SendTemplatedEmailAsync(
                    manager.Email,
                    $"Dispute Resolved: {order.OrderNumber}",
                    "inventory-order-alert",
                    new
                    {
                        Title = "Inventory Dispute Resolved",
                        Message = $"The dispute for order {order.OrderNumber} has been resolved. Your inventory balances have been updated according to the final agreed quantities.",
                        OrderNumber = order.OrderNumber,
                        Status = "Resolved",
                        SourceStore = order.SourceStoreId.HasValue ? "Referred Store" : "HQ",
                        DestinationStore = destinationStore?.Name ?? "Your Store",
                        ItemsHtml = itemsHtml,
                        ActionUrl = "#", 
                        ActionText = "View Final Order"
                    },
                    cancellationToken);
            }
        }
        catch (Exception)
        {
            // Ignore notification failures
        }
    }
}
