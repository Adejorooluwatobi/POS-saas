using System;
using System.Collections.Generic;

namespace POS.Application.DTOs.StockRequisition;

public class CreateStockRequisitionDto
{
    public string? Notes { get; set; }
    public List<CreateStockRequisitionItemDto> Items { get; set; } = [];
}

public class CreateStockRequisitionItemDto
{
    public Guid VariantId { get; set; }
    public int QuantityRequested { get; set; }
}
