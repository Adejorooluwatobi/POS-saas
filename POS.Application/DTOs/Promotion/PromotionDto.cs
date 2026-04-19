using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class PromotionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
    public PromotionType Type { get; set; }
    public PromotionScope Scope { get; set; }
    public decimal Value { get; set; }
    public decimal? MinPurchase { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
