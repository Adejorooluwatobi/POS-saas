namespace POS.Application.DTOs;

public class CreateTenantDto
{
    public string Slug { get; set; } = default!;
    public string BusinessName { get; set; } = default!;
    public string ContactEmail { get; set; } = default!;
    public string? ContactPhone { get; set; }
    public string Country { get; set; } = "Nigeria";
    public string? LogoUrl { get; set; }

    // Admin Account
    public string AdminEmail { get; set; } = default!;
    public string AdminPassword { get; set; } = default!;
    public string AdminFirstName { get; set; } = default!;
    public string AdminLastName { get; set; } = default!;

    // Initial Limits
    public int MaxStores { get; set; } = 1;
    public int MaxStaff { get; set; } = 5;
    public int MaxTerminals { get; set; } = 2;
}
