using System;
using System.Collections.Generic;
using POS.Domain.Enums;
using POS.Application.DTOs.InventoryOrder;

namespace POS.Application.DTOs.StockRequisition;

public class StockRequisitionDto
{
    public Guid Id { get; set; }
    public string RequisitionNumber { get; set; } = null!;
    public RequisitionStatus Status { get; set; }
    public Guid RequestingStoreId { get; set; }
    public string RequestingStoreName { get; set; } = null!;
    public Guid CreatedByStaffId { get; set; }
    public string CreatedByName { get; set; } = null!;
    public Guid? ReviewedByStaffId { get; set; }
    public string? ReviewedByName { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<StockRequisitionItemDto> Items { get; set; } = [];
    public List<InventoryOrderDto> FulfillmentOrders { get; set; } = [];
}

public class StockRequisitionItemDto
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public string VariantName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public int QuantityRequested { get; set; }
    public int QuantityFulfilled { get; set; }
}
