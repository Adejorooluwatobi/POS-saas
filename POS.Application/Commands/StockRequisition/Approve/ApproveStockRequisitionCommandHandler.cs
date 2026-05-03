using MediatR;
using POS.Application.DTOs.StockRequisition;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.StockRequisition.Approve;

public class ApproveStockRequisitionCommandHandler : IRequestHandler<ApproveStockRequisitionCommand>
{
    private readonly IStockRequisitionRepository _requisitionRepository;
    private readonly IInventoryOrderRepository _orderRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    private readonly IEmailService _emailService;
    private readonly IStaffRepository _staffRepository;
    private readonly IStoreRepository _storeRepository;

    public ApproveStockRequisitionCommandHandler(
        IStockRequisitionRepository requisitionRepository,
        IInventoryOrderRepository orderRepository,
        IUnitOfWork uow,
        ITenantContext tenantContext,
        IEmailService emailService,
        IStaffRepository staffRepository,
        IStoreRepository storeRepository)
    {
        _requisitionRepository = requisitionRepository;
        _orderRepository = orderRepository;
        _uow = uow;
        _tenantContext = tenantContext;
        _emailService = emailService;
        _staffRepository = staffRepository;
        _storeRepository = storeRepository;
    }

    public async Task Handle(ApproveStockRequisitionCommand request, CancellationToken cancellationToken)
    {
        var requisition = await _requisitionRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Requisition not found.");

        if (requisition.Status != RequisitionStatus.Pending && requisition.Status != RequisitionStatus.UnderReview)
            throw new InvalidOperationException("Requisition is already processed.");

        if (!_tenantContext.IsSuperAdmin && _tenantContext.SystemRole != "TenantAdmin" && _tenantContext.SystemRole != "Manager")
            throw new UnauthorizedAccessException("Only Generals can approve requisitions.");

        var staffId = _tenantContext.UserId ?? throw new UnauthorizedAccessException("User context missing.");

        foreach (var plan in request.Dto.FulfillmentPlans)
        {
            var orderNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            
            var order = new Domain.Entities.InventoryOrder
            {
                TenantId = requisition.TenantId,
                OrderNumber = orderNumber,
                Type = plan.SourceStoreId.HasValue ? InventoryOrderType.StoreToStore : InventoryOrderType.HqToStore,
                Status = InventoryOrderStatus.Draft,
                SourceStoreId = plan.SourceStoreId,
                DestinationStoreId = requisition.RequestingStoreId,
                CreatedByStaffId = staffId,
                StockRequisitionId = requisition.Id,
                IsReferredTransfer = plan.SourceStoreId.HasValue,
                Items = plan.Items.Select(i => new InventoryOrderItem
                {
                    InventoryOrderId = Guid.Empty,
                    VariantId = i.VariantId,
                    QuantityOrdered = i.Quantity
                }).ToList()
            };

            await _orderRepository.AddAsync(order);
        }

        requisition.Status = RequisitionStatus.Approved;
        requisition.ReviewedByStaffId = staffId;
        requisition.ReviewedAt = DateTimeOffset.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);

        // ── Trigger Email Notifications ──────────────────────────────────
        try
        {
            var requestingStore = await _storeRepository.GetByIdAsync(requisition.RequestingStoreId);
            var requestingManagers = await _staffRepository.GetManagersByStoreAsync(requisition.RequestingStoreId);

            // 1. Notify Requesting Store
            foreach (var manager in requestingManagers)
            {
                await _emailService.SendTemplatedEmailAsync(
                    manager.Email,
                    $"Requisition Approved: {requisition.RequisitionNumber}",
                    "inventory-order-alert",
                    new
                    {
                        Title = "Stock Requisition Approved",
                        Message = $"Your requisition {requisition.RequisitionNumber} has been approved. Fulfillment orders have been created and are being prepared.",
                        OrderNumber = requisition.RequisitionNumber,
                        Status = "Approved",
                        SourceStore = "HQ/Multiple",
                        DestinationStore = requestingStore?.Name ?? "Your Store",
                        ItemsHtml = "See portal for details",
                        ActionUrl = "#",
                        ActionText = "View Requisition"
                    },
                    cancellationToken);
            }

            // 2. Notify Source Stores (if any)
            foreach (var plan in request.Dto.FulfillmentPlans)
            {
                if (plan.SourceStoreId.HasValue)
                {
                    var sourceManagers = await _staffRepository.GetManagersByStoreAsync(plan.SourceStoreId.Value);
                    var sourceStore = await _storeRepository.GetByIdAsync(plan.SourceStoreId.Value);

                    foreach (var manager in sourceManagers)
                    {
                        await _emailService.SendTemplatedEmailAsync(
                            manager.Email,
                            $"New Stock Referral: {requisition.RequisitionNumber}",
                            "inventory-order-alert",
                            new
                            {
                                Title = "Stock Referral Request",
                                Message = $"HQ has referred a stock requisition from {requestingStore?.Name} to your store ({sourceStore?.Name}). Please review and accept the referral.",
                                OrderNumber = requisition.RequisitionNumber,
                                Status = "Pending Referral",
                                SourceStore = sourceStore?.Name ?? "Your Store",
                                DestinationStore = requestingStore?.Name ?? "Destination Store",
                                ItemsHtml = "See portal for details",
                                ActionUrl = "#",
                                ActionText = "Review Referral"
                            },
                            cancellationToken);
                    }
                }
            }
        }
        catch (Exception) { }
    }
}
