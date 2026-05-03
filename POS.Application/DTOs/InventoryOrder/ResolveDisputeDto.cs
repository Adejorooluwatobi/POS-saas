using System;
using System.Collections.Generic;

namespace POS.Application.DTOs.InventoryOrder;

public class ResolveDisputeDto
{
    public List<ResolveItemDto> Items { get; set; } = [];
}

public class ResolveItemDto
{
    public Guid ItemId { get; set; }
    public int FinalAgreedQuantity { get; set; }
}
