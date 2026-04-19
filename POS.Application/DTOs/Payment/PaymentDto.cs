using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid? GiftCardId { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public decimal? AmountTendered { get; set; }
    public decimal? ChangeGiven { get; set; }
    public PaymentStatus Status { get; set; }
    public string? GatewayRef { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
