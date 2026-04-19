namespace POS.Application.DTOs;

public class CreateCustomerDto
{
    public string? LoyaltyCardNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
