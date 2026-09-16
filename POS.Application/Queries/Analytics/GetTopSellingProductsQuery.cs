using MediatR;
using System;
using System.Collections.Generic;

namespace POS.Application.Queries.Analytics;

public class GetTopSellingProductsQuery : IRequest<List<TopSellingProductDto>>
{
    public Guid? StoreId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public int Limit { get; set; } = 10;
}

public class TopSellingProductDto
{
    public Guid? VariantId { get; set; }
    public string? ProductName { get; set; }
    public decimal TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}
