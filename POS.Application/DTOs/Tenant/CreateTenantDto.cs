namespace POS.Application.DTOs;

public class CreateTenantDto
{
    public string Slug { get; set; } = default!;
    public string BusinessName { get; set; } = default!;
    public string ContactEmail { get; set; } = default!;
    public string? ContactPhone { get; set; }
    public string Country { get; set; } = "Nigeria";
    public string? LogoUrl { get; set; }
}
