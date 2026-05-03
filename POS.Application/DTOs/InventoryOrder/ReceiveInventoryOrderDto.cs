using System;
using System.Collections.Generic;

namespace POS.Application.DTOs.InventoryOrder;

public class ReceiveInventoryOrderDto
{
    public List<ReceiveItemDto> Items { get; set; } = [];
}

public class ReceiveItemDto
{
    public Guid ItemId { get; set; }
    public int QuantityReceived { get; set; }
    public int? QuantityDamaged { get; set; }
    public string? DamageNotes { get; set; }
    public string? DamagePhotoUrl { get; set; }
}
