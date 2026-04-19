namespace POS.Application.DTOs;

public class CouponDto
{
    public Guid Id { get; set; }
    public Guid PromotionId { get; set; }
    public string Code { get; set; } = default!;
    public int MaxUses { get; set; }
    public int UsedCount { get; set; }
    public bool SingleUsePerCustomer { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCouponDto
{
    public string Code { get; set; } = default!;
    public int MaxUses { get; set; } = 1;
    public bool SingleUsePerCustomer { get; set; } = true;
    public DateTimeOffset? ExpiresAt { get; set; }
}

public class UpdateCouponDto
{
    public Guid Id { get; set; }
    public int MaxUses { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
