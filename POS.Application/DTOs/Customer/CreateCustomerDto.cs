namespace POS.Application.DTOs;

public class CreateCustomerDto
{
    public string? LoyaltyCardNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public POS.Domain.Enums.IdentityType IdentityType { get; set; }
    public string? IdentityNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public bool LivenessVerified { get; set; }
    public string? LivenessAuditLog { get; set; }
    public Guid? RegisteredStoreId { get; set; }
    public bool? IsSelfRegistered { get; set; }
}
