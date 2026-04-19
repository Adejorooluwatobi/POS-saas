using System;
using MediatR;

namespace POS.Application.Commands.Transaction.Delete;

public record DeleteTransactionCommand(Guid Id) : IRequest;
