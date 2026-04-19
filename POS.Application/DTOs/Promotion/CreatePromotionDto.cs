using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class CreatePromotionDto
{
    public string Name { get; set; } = default!;
    public PromotionType Type { get; set; }
    public PromotionScope Scope { get; set; }
    public decimal Value { get; set; }
    public decimal? MinPurchase { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? MaxUses { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
}
