using System;
using System.Text.Json;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class Payment : BaseEntity
{
    public required Guid TransactionId { get; set; }
    public Guid? GiftCardId { get; set; }
    public required PaymentMethod Method { get; set; }
    public required decimal Amount { get; set; }
    public decimal? AmountTendered { get; set; }
    public decimal? ChangeGiven { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? GatewayRef { get; set; }
    
    // Stored as JSONB
    public JsonDocument? GatewayResponse { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Transaction Transaction { get; set; } = null!;
    public GiftCard? GiftCard { get; set; }
}
