using MediatR;
using System;
using System.Collections.Generic;
using POS.Domain.Enums;

namespace POS.Application.Commands.Transaction.SyncOfflineTransactions;

public class SyncOfflineTransactionsCommand : IRequest<SyncOfflineTransactionsResult>
{
    public required List<OfflineTransactionDto> Transactions { get; set; }
}

public class SyncOfflineTransactionsResult
{
    public bool Success { get; set; }
    public int SyncedCount { get; set; }
    public List<Guid> FailedTransactionIds { get; set; } = [];
}

public class OfflineTransactionDto
{
    public required Guid Id { get; set; }
    public required string ReceiptNumber { get; set; }
    public required Guid SessionId { get; set; }
    public required Guid StoreId { get; set; }
    public required Guid CashierId { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    
    public List<OfflineTransactionItemDto> Items { get; set; } = [];
    public List<OfflinePaymentDto> Payments { get; set; } = [];
}

public class OfflineTransactionItemDto
{
    public required Guid Id { get; set; }
    public Guid? VariantId { get; set; }
    public string? ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class OfflinePaymentDto
{
    public required Guid Id { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? GatewayRef { get; set; }
    public decimal? AmountTendered { get; set; }
    public decimal? ChangeGiven { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}
