using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Customer.GetPaged;

public record GetCustomersPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<CustomerDto>>;
