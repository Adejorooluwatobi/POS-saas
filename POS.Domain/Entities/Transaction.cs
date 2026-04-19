using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

[Auditable]
public class Transaction : BaseEntity
{
    public required string ReceiptNumber { get; set; }
    public required Guid SessionId { get; set; }
    public required Guid StoreId { get; set; }
    public required Guid CashierId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? VoidRefId { get; set; }
    public TransactionType Type { get; set; } = TransactionType.Sale;
    public TransactionStatus Status { get; set; } = TransactionStatus.Open;
    public decimal Subtotal { get; set; } = 0m;
    public decimal DiscountTotal { get; set; } = 0m;
    public decimal TaxTotal { get; set; } = 0m;
    public decimal GrandTotal { get; set; } = 0m;
    public decimal AmountPaid { get; set; } = 0m;
    public decimal ChangeGiven { get; set; } = 0m;
    public int PointsEarned { get; set; } = 0;
    public int PointsRedeemed { get; set; } = 0;
    public string? Notes { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public TillSession Session { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public Staff Cashier { get; set; } = null!;
    public Customer? Customer { get; set; }
    public Transaction? VoidRef { get; set; }
    public ICollection<TransactionItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<AppliedDiscount> AppliedDiscounts { get; set; } = [];
}
