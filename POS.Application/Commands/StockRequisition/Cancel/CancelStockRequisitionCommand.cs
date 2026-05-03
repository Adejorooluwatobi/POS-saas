using MediatR;

namespace POS.Application.Commands.StockRequisition.Cancel;

public record CancelStockRequisitionCommand(Guid Id) : IRequest;
