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
    public IdentityType IdentityType { get; set; }
    public string? MaskedIdentityNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsIdentityVerified { get; set; }
    public DateTimeOffset? LivenessVerifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    // Registration Origin & Store
    public Guid? RegisteredStoreId { get; set; }
    public string? RegisteredStoreName { get; set; }
    public bool IsSelfRegistered { get; set; }
    public Guid? RegisteredByStaffId { get; set; }
    public string? RegisteredByStaffName { get; set; }

    // Summary Spending Statistics
    public decimal TotalSpend { get; set; }
    public int TotalVisits { get; set; }

    public ICollection<GiftCardDto> GiftCards { get; set; } = [];
}
