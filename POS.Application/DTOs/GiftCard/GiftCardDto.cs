namespace POS.Application.DTOs;

public class GiftCardDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CardNumber { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal InitialValue { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
}

public class IssueGiftCardDto
{
    public string CardNumber { get; set; } = default!;
    public decimal InitialValue { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public string? Pin { get; set; }
}

public class RedeemGiftCardDto
{
    public string CardNumber { get; set; } = default!;
    public string? Pin { get; set; }
    public decimal Amount { get; set; }
}
