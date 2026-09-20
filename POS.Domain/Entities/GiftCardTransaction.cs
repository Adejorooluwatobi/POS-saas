using System;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

[Auditable]
public class GiftCardTransaction : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required Guid GiftCardId { get; set; }
    public required GiftCardTransactionType Type { get; set; }
    public required decimal Amount { get; set; }
    public required decimal BalanceBefore { get; set; }
    public required decimal BalanceAfter { get; set; }
    public required PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public string? Reference { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? StaffId { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public GiftCard GiftCard { get; set; } = null!;
    public Store? Store { get; set; }
    public Staff? Staff { get; set; }
}
