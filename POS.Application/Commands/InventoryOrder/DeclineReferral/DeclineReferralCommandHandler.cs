using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.InventoryOrder.DeclineReferral;

public class DeclineReferralCommandHandler : IRequestHandler<DeclineReferralCommand>
{
    private readonly IInventoryOrderRepository _orderRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public DeclineReferralCommandHandler(
        IInventoryOrderRepository orderRepository,
        IUnitOfWork uow,
        ITenantContext tenantContext)
    {
        _orderRepository = orderRepository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(DeclineReferralCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!order.IsReferredTransfer)
            throw new InvalidOperationException("Only referred orders can be declined.");

        if (order.ReferralAccepted || order.Status != InventoryOrderStatus.Draft)
            throw new InvalidOperationException("Referral cannot be declined at this stage.");

        // Verification: only source store staff can decline
        if (_tenantContext.StoreId.HasValue && order.SourceStoreId != _tenantContext.StoreId.Value)
            throw new UnauthorizedAccessException("You can only decline referrals for your own store.");

        // Log the reason somewhere? Maybe order notes?
        order.Notes = $"Referral Declined. Reason: {request.Reason}";
        
        // We delete the order so HQ can find another source
        _orderRepository.Delete(order);

        await _uow.SaveChangesAsync(cancellationToken);

        // TODO: Notify HQ (TenantAdmin) that the referral was declined
    }
}
