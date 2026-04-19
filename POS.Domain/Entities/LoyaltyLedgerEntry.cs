using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class LoyaltyLedgerEntry : BaseEntity
{
    public required Guid CustomerId { get; set; }
    public Guid? TransactionId { get; set; }
    public required int Delta { get; set; }
    public required string Reason { get; set; }
    public required int BalanceAfter { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Transaction? Transaction { get; set; }
}
