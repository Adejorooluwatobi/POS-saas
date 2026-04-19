using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class UpdatePromotionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Value { get; set; }
    public decimal? MinPurchase { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? MaxUses { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public bool IsActive { get; set; }
}
