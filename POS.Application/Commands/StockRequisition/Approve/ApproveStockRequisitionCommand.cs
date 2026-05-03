using MediatR;
using POS.Application.DTOs.StockRequisition;

namespace POS.Application.Commands.StockRequisition.Approve;

public record ApproveStockRequisitionCommand(Guid Id, ApproveRequisitionDto Dto) : IRequest;
