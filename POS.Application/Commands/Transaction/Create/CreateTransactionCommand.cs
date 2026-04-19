using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Transaction.Create;

public record CreateTransactionCommand(CreateTransactionDto Dto) : IRequest<TransactionDto>;
