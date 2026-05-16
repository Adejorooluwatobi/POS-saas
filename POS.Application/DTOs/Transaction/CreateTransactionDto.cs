using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class CreateTransactionDto
{
    public Guid SessionId { get; set; }
    public Guid StoreId { get; set; }
    public Guid? CustomerId { get; set; }
    public TransactionType Type { get; set; } = TransactionType.Sale;
    public string? Notes { get; set; }
    public List<CreateTransactionItemDto> Items { get; set; } = [];
    public List<CreatePaymentDto> Payments { get; set; } = [];
}

public class CreateTransactionItemDto
{
    public Guid VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public string? Name { get; set; }
    
    // Gift Card Issuance
    public bool IsGiftCardSale { get; set; }
    public string? GiftCardNumber { get; set; }
    public string? GiftCardPin { get; set; }
}
