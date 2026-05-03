using MediatR;

namespace POS.Application.Commands.StockRequisition.Reject;

public record RejectStockRequisitionCommand(Guid Id, string Reason) : IRequest;
