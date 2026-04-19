namespace POS.Application.DTOs;

public class UpdateTenantDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = default!;
    public string ContactEmail { get; set; } = default!;
    public string? ContactPhone { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
}
