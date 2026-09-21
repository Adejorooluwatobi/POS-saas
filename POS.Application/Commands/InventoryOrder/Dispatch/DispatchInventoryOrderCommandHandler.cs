using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.InventoryOrder.Dispatch;

public class DispatchInventoryOrderCommandHandler : IRequestHandler<DispatchInventoryOrderCommand>
{
    private readonly IInventoryOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;
    private readonly IEmailService _emailService;
    private readonly IStaffRepository _staffRepository;
    private readonly IStoreRepository _storeRepository;

    public DispatchInventoryOrderCommandHandler(
        IInventoryOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IProductVariantRepository variantRepository,
        IStockMovementRepository movementRepository,
        IUnitOfWork uow,
        ITenantContext tenantContext,
        IEmailService emailService,
        IStaffRepository staffRepository,
        IStoreRepository storeRepository)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _variantRepository = variantRepository;
        _movementRepository = movementRepository;
        _uow = uow;
        _tenantContext = tenantContext;
        _emailService = emailService;
        _staffRepository = staffRepository;
        _storeRepository = storeRepository;
    }

    public async Task Handle(DispatchInventoryOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id) 
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Status != InventoryOrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be dispatched.");

        // If it's a store-to-store transfer, deduct from source store
        if (order.SourceStoreId.HasValue)
        {
            foreach (var item in order.Items)
            {
                var variant = await _variantRepository.GetByIdAsync(item.VariantId)
                    ?? throw new KeyNotFoundException($"Variant {item.VariantId} not found.");
                
                var baseVariantId = variant.IsBaseUnit ? variant.Id : variant.BaseVariantId!.Value;
                var qtyInBaseUnits = item.QuantityOrdered;

                var inventory = await _inventoryRepository.GetByVariantAndStoreAsync(baseVariantId, order.SourceStoreId.Value);
                if (inventory == null || inventory.QuantityOnHand < qtyInBaseUnits)
                {
                    var available = inventory?.QuantityOnHand ?? 0;
                    throw new InvalidOperationException($"Insufficient stock for {variant.Sku} at source store. You only have {available} available.");
                }

                inventory.QuantityOnHand -= qtyInBaseUnits;
                
                var movement = new Domain.Entities.StockMovement
                {
                    TenantId = order.TenantId,
                    StoreId = order.SourceStoreId.Value,
                    VariantId = baseVariantId,
                    QuantityChange = -qtyInBaseUnits,
                    BalanceAfter = inventory.QuantityOnHand,
                    Reason = $"Dispatched Order {order.OrderNumber}",
                    ReferenceId = order.Id,
                    ReferenceType = "InventoryOrder"
                };
                
                await _movementRepository.AddAsync(movement);
            }
        }

        order.Status = InventoryOrderStatus.Dispatched;
        order.DispatchedAt = request.Dto?.DispatchedAt ?? DateTimeOffset.UtcNow;
        if (request.Dto != null)
        {
            if (!string.IsNullOrWhiteSpace(request.Dto.DriverName))
                order.DriverName = request.Dto.DriverName.Trim();
            if (!string.IsNullOrWhiteSpace(request.Dto.DriverPhone))
                order.DriverPhone = request.Dto.DriverPhone.Trim();
            if (!string.IsNullOrWhiteSpace(request.Dto.VehiclePlateNumber))
                order.VehiclePlateNumber = request.Dto.VehiclePlateNumber.Trim();
            if (request.Dto.EstimatedDeliveryTime.HasValue)
                order.EstimatedDeliveryTime = request.Dto.EstimatedDeliveryTime.Value;
            if (!string.IsNullOrWhiteSpace(request.Dto.DispatchNotes))
            {
                order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                    ? request.Dto.DispatchNotes.Trim()
                    : $"{order.Notes}\n[Dispatch Note]: {request.Dto.DispatchNotes.Trim()}";
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);

        // ── Trigger Email Notification ───────────────────────────────────
        try
        {
            var destinationStore = await _storeRepository.GetByIdAsync(order.DestinationStoreId);
            var sourceStoreName = order.SourceStoreId.HasValue 
                ? (await _storeRepository.GetByIdAsync(order.SourceStoreId.Value))?.Name ?? "Store"
                : "HQ";

            var managers = await _staffRepository.GetManagersByStoreAsync(order.DestinationStoreId);
            
            var itemsHtml = string.Join("", order.Items.Select(i => 
                $"<tr><td style='padding: 8px 0; border-bottom: 1px solid #F1F5F9;'>{i.Variant?.Sku ?? "Item"}</td>" +
                $"<td style='padding: 8px 0; border-bottom: 1px solid #F1F5F9;'>{i.QuantityOrdered}</td></tr>"));

            var logisticsDetails = "";
            if (!string.IsNullOrWhiteSpace(order.DriverName))
                logisticsDetails += $" Driver: {order.DriverName}" + (!string.IsNullOrWhiteSpace(order.DriverPhone) ? $" ({order.DriverPhone})" : "");
            if (!string.IsNullOrWhiteSpace(order.VehiclePlateNumber))
                logisticsDetails += $" | Vehicle Plate: {order.VehiclePlateNumber}";
            if (order.EstimatedDeliveryTime.HasValue)
                logisticsDetails += $" | Est. Delivery: {order.EstimatedDeliveryTime.Value:g}";

            foreach (var manager in managers)
            {
                await _emailService.SendTemplatedEmailAsync(
                    manager.Email,
                    $"Inventory Dispatched: {order.OrderNumber}",
                    "inventory-order-alert",
                    new
                    {
                        Title = "New Inbound Shipment",
                        Message = $"A new inventory order has been dispatched from {sourceStoreName} to your store ({destinationStore?.Name}).{logisticsDetails} Please prepare to receive it.",
                        OrderNumber = order.OrderNumber,
                        Status = "Dispatched",
                        SourceStore = sourceStoreName,
                        DestinationStore = destinationStore?.Name ?? "Your Store",
                        ItemsHtml = itemsHtml,
                        ActionUrl = "#", // TODO: Add frontend link
                        ActionText = "View Order Details"
                    },
                    cancellationToken);
            }
        }
        catch (Exception)
        {
            // Don't fail the command if email fails
            // _logger.LogError(ex, "Failed to send dispatch notifications");
        }
    }
}
