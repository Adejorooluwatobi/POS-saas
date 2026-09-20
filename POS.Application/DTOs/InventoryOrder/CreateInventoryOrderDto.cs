using System;
using System.Collections.Generic;
using POS.Domain.Enums;

namespace POS.Application.DTOs.InventoryOrder;

public class CreateInventoryOrderDto
{
    public InventoryOrderType Type { get; set; }
    public Guid? SourceStoreId { get; set; }
    public Guid DestinationStoreId { get; set; }
    public string? Notes { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? VehiclePlateNumber { get; set; }
    public DateTimeOffset? EstimatedDeliveryTime { get; set; }
    public Guid? StockRequisitionId { get; set; }
    public List<CreateInventoryOrderItemDto> Items { get; set; } = [];
}

public class CreateInventoryOrderItemDto
{
    public Guid VariantId { get; set; }
    public int QuantityOrdered { get; set; }
    public string? BatchNumber { get; set; }
    public DateTimeOffset? ProductionDate { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
}
