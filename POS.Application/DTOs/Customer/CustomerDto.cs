using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? LoyaltyCardNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int PointsBalance { get; set; }
    public CustomerTier Tier { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
