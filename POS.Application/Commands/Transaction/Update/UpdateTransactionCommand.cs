using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Transaction.Update;

public record UpdateTransactionCommand(Guid Id, UpdateTransactionDto Dto) : IRequest;
