using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Customer.GetTransactions;

public class GetCustomerTransactionsQueryHandler : IRequestHandler<GetCustomerTransactionsQuery, CustomerTransactionHistoryDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IMapper _mapper;

    public GetCustomerTransactionsQueryHandler(
        ITransactionRepository transactionRepository,
        ICustomerRepository customerRepository,
        ITenantContext tenantContext,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _customerRepository = customerRepository;
        _tenantContext = tenantContext;
        _mapper = mapper;
    }

    public async Task<CustomerTransactionHistoryDto> Handle(GetCustomerTransactionsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Base query for customer transactions
        var query = _transactionRepository.GetQueryable()
            .Where(t => t.CustomerId == request.CustomerId);

        if (tenantId.HasValue)
        {
            query = query.Where(t => t.Store.TenantId == tenantId.Value);
        }

        var completedQuery = query.Where(t => t.Status == TransactionStatus.Completed);

        // Aggregate statistics
        var lifetimeSpend = await completedQuery
            .SumAsync(t => (decimal?)t.GrandTotal, cancellationToken) ?? 0m;

        var totalVisits = await completedQuery
            .CountAsync(cancellationToken);

        var aov = totalVisits > 0 ? Math.Round(lifetimeSpend / totalVisits, 2) : 0m;

        // Payment method breakdown for this customer
        var payments = await query
            .Where(t => t.Status == TransactionStatus.Completed)
            .SelectMany(t => t.Payments)
            .Where(p => p.Status == PaymentStatus.Approved)
            .GroupBy(p => p.Method)
            .Select(g => new { Method = g.Key.ToString(), Total = g.Sum(p => p.Amount) })
            .ToListAsync(cancellationToken);

        var spendByPaymentMethod = payments.ToDictionary(p => p.Method, p => p.Total);

        // Paged transactions with navigations included
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(t => t.Store)
            .Include(t => t.Cashier)
            .Include(t => t.Items)
                .ThenInclude(i => i.Variant)
                    .ThenInclude(v => v!.Product)
            .Include(t => t.Payments)
            .ToListAsync(cancellationToken);

        return new CustomerTransactionHistoryDto
        {
            LifetimeSpend = lifetimeSpend,
            TotalVisits = totalVisits,
            AverageOrderValue = aov,
            SpendByPaymentMethod = spendByPaymentMethod,
            Transactions = new PagedResult<TransactionDto>
            {
                Items = _mapper.Map<IEnumerable<TransactionDto>>(items),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            }
        };
    }
}
