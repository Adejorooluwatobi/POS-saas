using System;

namespace POS.Application.DTOs.InventoryOrder;

public class DispatchInventoryOrderDto
{
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? VehiclePlateNumber { get; set; }
    public DateTimeOffset? DispatchedAt { get; set; }
    public DateTimeOffset? EstimatedDeliveryTime { get; set; }
    public string? DispatchNotes { get; set; }
}
