using MediatR;
using System;
using System.Collections.Generic;

namespace POS.Application.Queries.Analytics;

public class GetBusiestHoursQuery : IRequest<List<BusiestHourDto>>
{
    public Guid? StoreId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

public class BusiestHourDto
{
    public int HourOfDay { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
