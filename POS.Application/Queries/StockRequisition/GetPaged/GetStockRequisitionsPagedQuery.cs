using MediatR;
using POS.Application.DTOs.StockRequisition;
using POS.Domain.Common;

namespace POS.Application.Queries.StockRequisition.GetPaged;

public record GetStockRequisitionsPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<StockRequisitionDto>>;
