using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Transaction.GetById;

public record GetTransactionByIdQuery(Guid Id) : IRequest<TransactionDto?>;
