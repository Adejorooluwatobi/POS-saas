namespace POS.Application.DTOs;

public class GiftCardDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CardNumber { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal InitialValue { get; set; }
    public Guid? IssuingStoreId { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset IssuedAt { get; set; }

    // Linked Customer Info
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerLoyaltyCardNo { get; set; }
    public int? CustomerPointsBalance { get; set; }
}

public class IssueGiftCardDto
{
    public string CardNumber { get; set; } = default!;
    public decimal InitialValue { get; set; }
    public Guid? IssuingStoreId { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public string? Pin { get; set; }

    // Optional customer linking at issuance
    public Guid? CustomerId { get; set; }
    public string? PaymentMethod { get; set; } // Cash, BankTransfer, etc.
    public string? Reference { get; set; }
}

public class RedeemGiftCardDto
{
    public string CardNumber { get; set; } = default!;
    public string? Pin { get; set; }
    public decimal Amount { get; set; }
}

public class GiftCardTransactionDto
{
    public Guid Id { get; set; }
    public Guid GiftCardId { get; set; }
    public string CardNumber { get; set; } = default!;
    public string Type { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Reference { get; set; }
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
    public Guid? StaffId { get; set; }
    public string? StaffName { get; set; }
    public Guid? TargetGiftCardId { get; set; }
    public string? TargetCardNumber { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class RechargeGiftCardDto
{
    public string CardNumber { get; set; } = default!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash"; // Cash, BankTransfer, MobileMoney
    public string? Reference { get; set; }
    public Guid? StoreId { get; set; }
    public string? Notes { get; set; }
}

public class TransferCardBalanceDto
{
    public string SourceCardNumber { get; set; } = default!;
    public string? SourcePin { get; set; }
    public string DestinationCardNumber { get; set; } = default!;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class LinkGiftCardCustomerDto
{
    public Guid CustomerId { get; set; }
}
