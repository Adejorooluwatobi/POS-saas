using MediatR;
using POS.Application.DTOs.InventoryOrder;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.InventoryOrder.Dispute;

public class DisputeInventoryOrderCommandHandler : IRequestHandler<DisputeInventoryOrderCommand>
{
    private readonly IInventoryOrderRepository _orderRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;
    private readonly IEmailService _emailService;
    private readonly IStaffRepository _staffRepository;
    private readonly IStoreRepository _storeRepository;

    public DisputeInventoryOrderCommandHandler(
        IInventoryOrderRepository orderRepository,
        IUnitOfWork uow,
        ITenantContext tenantContext,
        IEmailService emailService,
        IStaffRepository staffRepository,
        IStoreRepository storeRepository)
    {
        _orderRepository = orderRepository;
        _uow = uow;
        _tenantContext = tenantContext;
        _emailService = emailService;
        _staffRepository = staffRepository;
        _storeRepository = storeRepository;
    }

    public async Task Handle(DisputeInventoryOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Status != InventoryOrderStatus.Received)
            throw new InvalidOperationException("Order must be received to be disputed.");

        // Verification: only StoreManager can dispute (based on user request)
        if (_tenantContext.SystemRole != "StoreManager" && !_tenantContext.IsSuperAdmin)
            throw new UnauthorizedAccessException("Only Store Managers can dispute inventory deliveries.");

        if (_tenantContext.StoreId.HasValue && order.DestinationStoreId != _tenantContext.StoreId.Value)
            throw new UnauthorizedAccessException("You can only dispute orders for your own store.");

        order.Status = InventoryOrderStatus.Disputed;
        order.DisputeNotes = request.Dto.DisputeNotes;
        order.DisputePhotoUrl = request.Dto.DisputePhotoUrl;

        await _uow.SaveChangesAsync(cancellationToken);

        // ── Trigger Email Notification ───────────────────────────────────
        try
        {
            var destinationStore = await _storeRepository.GetByIdAsync(order.DestinationStoreId);
            var generals = await _staffRepository.GetGeneralsAsync();
            var sourceManagers = order.SourceStoreId.HasValue 
                ? await _staffRepository.GetManagersByStoreAsync(order.SourceStoreId.Value)
                : Enumerable.Empty<POS.Domain.Entities.Staff>();

            var recipients = generals.Concat(sourceManagers).GroupBy(s => s.Email).Select(g => g.First());

            var itemsHtml = string.Join("", order.Items.Select(i => 
                $"<tr><td style='padding: 8px 0; border-bottom: 1px solid #F1F5F9;'>{i.Variant?.Sku ?? "Item"}</td>" +
                $"<td style='padding: 8px 0; border-bottom: 1px solid #F1F5F9;'>Ordered: {i.QuantityOrdered} | Received: {i.QuantityReceived}</td></tr>"));

            foreach (var recipient in recipients)
            {
                await _emailService.SendTemplatedEmailAsync(
                    recipient.Email,
                    $"Dispute Raised: {order.OrderNumber}",
                    "inventory-order-alert",
                    new
                    {
                        Title = "Inventory Dispute Raised",
                        Message = $"A dispute has been raised by {destinationStore?.Name} for order {order.OrderNumber}. Reason: {order.DisputeNotes}",
                        OrderNumber = order.OrderNumber,
                        Status = "Disputed",
                        SourceStore = order.SourceStoreId.HasValue ? "Referred Store" : "HQ",
                        DestinationStore = destinationStore?.Name ?? "Store",
                        ItemsHtml = itemsHtml,
                        ActionUrl = "#", 
                        ActionText = "Review Dispute"
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
