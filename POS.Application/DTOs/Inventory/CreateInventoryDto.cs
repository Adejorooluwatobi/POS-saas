namespace POS.Application.DTOs;

public class CreateInventoryDto
{
    public Guid VariantId { get; set; }
    public Guid StoreId { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQty { get; set; }
}
