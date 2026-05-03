using MediatR;
using POS.Application.DTOs.InventoryOrder;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.InventoryOrder.Receive;

public class ReceiveInventoryOrderCommandHandler : IRequestHandler<ReceiveInventoryOrderCommand>
{
    private readonly IInventoryOrderRepository _orderRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public ReceiveInventoryOrderCommandHandler(
        IInventoryOrderRepository orderRepository,
        IUnitOfWork uow,
        ITenantContext tenantContext)
    {
        _orderRepository = orderRepository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(ReceiveInventoryOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Status != InventoryOrderStatus.Dispatched)
            throw new InvalidOperationException("Order must be dispatched to be received.");

        // Verification: only destination store staff can receive
        if (_tenantContext.StoreId.HasValue && order.DestinationStoreId != _tenantContext.StoreId.Value)
            throw new UnauthorizedAccessException("You can only receive orders for your own store.");

        foreach (var itemDto in request.Dto.Items)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == itemDto.ItemId)
                ?? throw new KeyNotFoundException($"Item {itemDto.ItemId} not found in order.");

            item.QuantityReceived = itemDto.QuantityReceived;
            item.QuantityDamaged = itemDto.QuantityDamaged;
            item.DamageNotes = itemDto.DamageNotes;
            item.DamagePhotoUrl = itemDto.DamagePhotoUrl;
        }

        order.Status = InventoryOrderStatus.Received;
        order.ReceivedAt = DateTimeOffset.UtcNow;
        order.ReceivedByStaffId = _tenantContext.UserId;

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
