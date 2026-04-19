using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class EodReport : BaseEntity
{
    public required Guid StoreId { get; set; }
    public Guid? SessionId { get; set; }
    public required DateOnly ReportDate { get; set; }
    public decimal TotalSales { get; set; } = 0m;
    public decimal TotalReturns { get; set; } = 0m;
    public decimal NetSales { get; set; } = 0m;
    public decimal TotalTax { get; set; } = 0m;
    public decimal TotalDiscounts { get; set; } = 0m;
    public int TransactionCount { get; set; } = 0;
    public decimal CashCollected { get; set; } = 0m;
    public decimal CardCollected { get; set; } = 0m;
    public decimal OtherCollected { get; set; } = 0m;
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Store Store { get; set; } = null!;
}
