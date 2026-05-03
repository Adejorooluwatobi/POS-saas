using MediatR;
using POS.Application.DTOs.StockRequisition;

namespace POS.Application.Commands.StockRequisition.Create;

public record CreateStockRequisitionCommand(CreateStockRequisitionDto Dto) : IRequest<StockRequisitionDto>;
