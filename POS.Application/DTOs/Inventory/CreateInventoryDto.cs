namespace POS.Application.DTOs;

public class CreateInventoryDto
{
    public Guid VariantId { get; set; }
    public Guid StoreId { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQty { get; set; }
    public string? BatchNumber { get; set; }
    public DateTimeOffset? ProductionDate { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    public int? ExpiryAlertPercentage { get; set; }
}
