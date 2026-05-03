using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class InventoryOrder : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required string OrderNumber { get; set; }
    public InventoryOrderType Type { get; set; }
    public InventoryOrderStatus Status { get; set; }
    public Guid? SourceStoreId { get; set; }
    public required Guid DestinationStoreId { get; set; }
    public required Guid CreatedByStaffId { get; set; }
    public Guid? ReceivedByStaffId { get; set; }
    public Guid? ApprovedByStaffId { get; set; }
    public Guid? ResolvedByStaffId { get; set; }
    public DateTimeOffset? DispatchedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? Notes { get; set; }
    public string? DisputeNotes { get; set; }
    public string? DisputePhotoUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Fulfillment / Referral fields
    public Guid? StockRequisitionId { get; set; }
    public bool IsReferredTransfer { get; set; } = false;
    public bool ReferralAccepted { get; set; } = false;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Store? SourceStore { get; set; }
    public Store DestinationStore { get; set; } = null!;
    public Staff CreatedBy { get; set; } = null!;
    public Staff? ReceivedBy { get; set; }
    public Staff? ApprovedBy { get; set; }
    public Staff? ResolvedBy { get; set; }
    public StockRequisition? StockRequisition { get; set; }
    public ICollection<InventoryOrderItem> Items { get; set; } = [];
}
