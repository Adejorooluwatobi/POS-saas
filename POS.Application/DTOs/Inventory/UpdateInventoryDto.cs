namespace POS.Application.DTOs;

public class UpdateInventoryDto
{
    public Guid Id { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQty { get; set; }
}
