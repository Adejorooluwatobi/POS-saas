namespace POS.Application.DTOs;

public class CreateStoreDto
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? State { get; set; }
    public string Country { get; set; } = "Nigeria";
    public string? Phone { get; set; }
    public string Timezone { get; set; } = "Africa/Lagos";
}
