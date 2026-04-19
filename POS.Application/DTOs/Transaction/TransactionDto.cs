using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = default!;
    public Guid SessionId { get; set; }
    public Guid StoreId { get; set; }
    public Guid CashierId { get; set; }
    public Guid? CustomerId { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public int PointsEarned { get; set; }
    public int PointsRedeemed { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<TransactionItemDto> Items { get; set; } = [];
    public List<PaymentDto> Payments { get; set; } = [];
}

public class TransactionItemDto
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public bool IsVoided { get; set; }
}
