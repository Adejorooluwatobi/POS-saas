using MediatR;
using System;

namespace POS.Application.Queries.Analytics;

public class GetProfitMarginReportQuery : IRequest<ProfitMarginReportDto>
{
    public Guid? StoreId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

public class ProfitMarginReportDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalCostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal ProfitMarginPercentage { get; set; }
}
