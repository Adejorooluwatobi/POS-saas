using System;
using System.Collections.Generic;

namespace POS.Application.DTOs.StockRequisition;

public class ApproveRequisitionDto
{
    public List<FulfillmentPlanDto> FulfillmentPlans { get; set; } = [];
}

public class FulfillmentPlanDto
{
    public Guid? SourceStoreId { get; set; } // null = HQ
    public List<FulfillmentPlanItemDto> Items { get; set; } = [];
}

public class FulfillmentPlanItemDto
{
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTimeOffset? ProductionDate { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
}
