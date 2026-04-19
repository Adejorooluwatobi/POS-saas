namespace POS.Application.DTOs;

public class StoreDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? State { get; set; }
    public string Country { get; set; } = default!;
    public string? Phone { get; set; }
    public string Timezone { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
