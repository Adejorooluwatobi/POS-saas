using System;
using System.Collections.Generic;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class Coupon : BaseEntity
{
    public required Guid PromotionId { get; set; }
    public required string Code { get; set; }
    public int MaxUses { get; set; } = 1;
    public int UsedCount { get; set; } = 0;
    public bool SingleUsePerCustomer { get; set; } = true;
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Promotion Promotion { get; set; } = null!;
    public ICollection<AppliedDiscount> AppliedDiscounts { get; set; } = [];
}
