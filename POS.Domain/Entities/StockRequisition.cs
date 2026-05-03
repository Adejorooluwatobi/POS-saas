using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class StockRequisition : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required string RequisitionNumber { get; set; }
    public RequisitionStatus Status { get; set; }
    public required Guid RequestingStoreId { get; set; }
    public required Guid CreatedByStaffId { get; set; }
    public Guid? ReviewedByStaffId { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Store RequestingStore { get; set; } = null!;
    public Staff CreatedBy { get; set; } = null!;
    public Staff? ReviewedBy { get; set; }
    public ICollection<StockRequisitionItem> Items { get; set; } = [];
    public ICollection<InventoryOrder> FulfillmentOrders { get; set; } = [];
}
