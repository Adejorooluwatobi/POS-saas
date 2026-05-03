using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.InventoryOrder.AcceptReferral;

public class AcceptReferralCommandHandler : IRequestHandler<AcceptReferralCommand>
{
    private readonly IInventoryOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public AcceptReferralCommandHandler(
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

    public async Task Handle(AcceptReferralCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!order.IsReferredTransfer)
            throw new InvalidOperationException("Only referred orders can be accepted via this command.");

        if (order.ReferralAccepted)
            throw new InvalidOperationException("Referral already accepted.");

        // Verification: only source store staff can accept
        if (_tenantContext.StoreId.HasValue && order.SourceStoreId != _tenantContext.StoreId.Value)
            throw new UnauthorizedAccessException("You can only accept referrals for your own store.");

        // Deduct from source store (now that it's accepted and dispatched)
        foreach (var item in order.Items)
        {
            var variant = await _variantRepository.GetByIdAsync(item.VariantId)
                ?? throw new KeyNotFoundException($"Variant {item.VariantId} not found.");

            var baseVariantId = variant.IsBaseUnit ? variant.Id : variant.BaseVariantId!.Value;
            var qtyInBaseUnits = (int)(item.QuantityOrdered * variant.ConversionFactor);

            var inventory = await _inventoryRepository.GetByVariantAndStoreAsync(baseVariantId, order.SourceStoreId!.Value);
            if (inventory != null)
            {
                inventory.QuantityOnHand -= qtyInBaseUnits;
            }
        }

        order.ReferralAccepted = true;
        order.Status = InventoryOrderStatus.Dispatched;
        order.DispatchedAt = DateTimeOffset.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
