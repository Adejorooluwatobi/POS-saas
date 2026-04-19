using System;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class AppliedDiscount : BaseEntity
{
    public required Guid TransactionId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? PromotionId { get; set; }
    public Guid? CouponId { get; set; }
    public Guid? AppliedById { get; set; }
    public required DiscountType Type { get; set; }
    public required decimal Amount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Transaction Transaction { get; set; } = null!;
    public TransactionItem? Item { get; set; }
    public Promotion? Promotion { get; set; }
    public Coupon? Coupon { get; set; }
    public Staff? AppliedBy { get; set; }
}
