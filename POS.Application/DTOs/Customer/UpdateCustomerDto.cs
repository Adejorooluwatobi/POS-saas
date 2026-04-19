namespace POS.Application.DTOs;

public class UpdateCustomerDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
}
