namespace POS.Application.DTOs;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string BusinessName { get; set; } = default!;
    public string ContactEmail { get; set; } = default!;
    public string? ContactPhone { get; set; }
    public string Country { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
