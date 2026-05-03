using MediatR;
using POS.Application.DTOs.StockRequisition;

namespace POS.Application.Queries.StockRequisition.GetById;

public record GetStockRequisitionByIdQuery(Guid Id) : IRequest<StockRequisitionDto>;
