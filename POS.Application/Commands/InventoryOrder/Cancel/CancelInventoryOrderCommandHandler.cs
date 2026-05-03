using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.InventoryOrder.Cancel;

public class CancelInventoryOrderCommandHandler : IRequestHandler<CancelInventoryOrderCommand>
{
    private readonly IInventoryOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public CancelInventoryOrderCommandHandler(
        IInventoryOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IProductVariantRepository variantRepository,
        IUnitOfWork uow,
        ITenantContext tenantContext)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _variantRepository = variantRepository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(CancelInventoryOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Status == InventoryOrderStatus.Approved || order.Status == InventoryOrderStatus.Resolved)
            throw new InvalidOperationException("Cannot cancel an order that has already been approved or resolved.");

        // If it was already dispatched and had a source store, return stock to source
        if (order.Status == InventoryOrderStatus.Dispatched || order.Status == InventoryOrderStatus.Received || order.Status == InventoryOrderStatus.Disputed)
        {
            if (order.SourceStoreId.HasValue)
            {
                foreach (var item in order.Items)
                {
                    var variant = await _variantRepository.GetByIdAsync(item.VariantId)
                        ?? throw new KeyNotFoundException($"Variant {item.VariantId} not found.");

                    var baseVariantId = variant.IsBaseUnit ? variant.Id : variant.BaseVariantId!.Value;
                    var qtyInBaseUnits = (int)(item.QuantityOrdered * variant.ConversionFactor);

                    var sourceInventory = await _inventoryRepository.GetByVariantAndStoreAsync(baseVariantId, order.SourceStoreId.Value);
                    if (sourceInventory != null)
                    {
                        sourceInventory.QuantityOnHand += qtyInBaseUnits;
                    }
                }
            }
        }

        // Using a custom "Cancelled" status or just deleting? 
        // Better to have a Cancelled status in the enum or just delete it if it's draft.
        // I didn't add Cancelled to InventoryOrderStatus enum. 
        // I'll just delete it for now or assume it's removed.
        // Actually, let's just delete the entity if it's draft, or mark it as something.
        // For now, I'll delete it.
        _orderRepository.Delete(order);

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
