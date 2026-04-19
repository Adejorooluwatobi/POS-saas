using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Payment.GetPaged;

public record GetPaymentsPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<PaymentDto>>;
