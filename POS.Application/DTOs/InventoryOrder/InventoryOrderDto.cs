using System;
using System.Collections.Generic;
using POS.Domain.Enums;

namespace POS.Application.DTOs.InventoryOrder;

public class InventoryOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public InventoryOrderType Type { get; set; }
    public InventoryOrderStatus Status { get; set; }
    public Guid? SourceStoreId { get; set; }
    public string? SourceStoreName { get; set; }
    public Guid DestinationStoreId { get; set; }
    public string DestinationStoreName { get; set; } = null!;
    public Guid CreatedByStaffId { get; set; }
    public string CreatedByName { get; set; } = null!;
    public Guid? ReceivedByStaffId { get; set; }
    public string? ReceivedByName { get; set; }
    public Guid? ApprovedByStaffId { get; set; }
    public string? ApprovedByName { get; set; }
    public Guid? ResolvedByStaffId { get; set; }
    public string? ResolvedByName { get; set; }
    public DateTimeOffset? DispatchedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset? EstimatedDeliveryTime { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? VehiclePlateNumber { get; set; }
    public string? Notes { get; set; }
    public string? DisputeNotes { get; set; }
    public string? DisputePhotoUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Guid? StockRequisitionId { get; set; }
    public bool IsReferredTransfer { get; set; }
    public bool ReferralAccepted { get; set; }

    public List<InventoryOrderItemDto> Items { get; set; } = [];
}

public class InventoryOrderItemDto
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public string VariantName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public int QuantityOrdered { get; set; }
    public int? QuantityReceived { get; set; }
    public int? QuantityReceivedBaseUnits { get; set; }
    public int? QuantityDamaged { get; set; }
    public string? DamageNotes { get; set; }
    public string? DamagePhotoUrl { get; set; }
    
    // Packaging info for UI
    public decimal ConversionFactor { get; set; }
    public int? SinglesPerRoll { get; set; }
    public int? RollsPerPack { get; set; }
    public int? SinglesPerPack { get; set; }
    public string? BatchNumber { get; set; }
    public DateTimeOffset? ProductionDate { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
}
