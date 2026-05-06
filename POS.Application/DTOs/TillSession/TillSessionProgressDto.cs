namespace POS.Application.DTOs;

public class TillSessionProgressDto
{
    public Guid SessionId { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal ExpectedCash { get; set; }
    public int TransactionCount { get; set; }
    public List<PaymentSummaryDto> Payments { get; set; } = [];
}

public class PaymentSummaryDto
{
    public string Method { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
