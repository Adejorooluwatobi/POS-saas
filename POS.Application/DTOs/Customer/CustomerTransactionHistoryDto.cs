using System.Collections.Generic;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.DTOs;

public class CustomerTransactionHistoryDto
{
    public decimal LifetimeSpend { get; set; }
    public int TotalVisits { get; set; }
    public decimal AverageOrderValue { get; set; }
    public Dictionary<string, decimal> SpendByPaymentMethod { get; set; } = new();
    public PagedResult<TransactionDto> Transactions { get; set; } = new();
}
