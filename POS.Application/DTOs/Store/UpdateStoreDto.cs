namespace POS.Application.DTOs;

public class UpdateStoreDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? State { get; set; }
    public string? Phone { get; set; }
    public string Timezone { get; set; } = default!;
    public bool IsActive { get; set; }
}
