namespace POS.Application.DTOs;

public class InventoryDto
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public Guid StoreId { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityAvailable { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQty { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
