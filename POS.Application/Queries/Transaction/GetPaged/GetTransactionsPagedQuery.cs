using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Transaction.GetPaged;

public record GetTransactionsPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<TransactionDto>>;
