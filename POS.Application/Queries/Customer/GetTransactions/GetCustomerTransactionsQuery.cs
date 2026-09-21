using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Customer.GetTransactions;

public record GetCustomerTransactionsQuery(Guid CustomerId, int PageNumber = 1, int PageSize = 20) : IRequest<CustomerTransactionHistoryDto>;
