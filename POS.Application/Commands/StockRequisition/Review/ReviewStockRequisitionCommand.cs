using MediatR;

namespace POS.Application.Commands.StockRequisition.Review;

public record ReviewStockRequisitionCommand(Guid Id) : IRequest;
