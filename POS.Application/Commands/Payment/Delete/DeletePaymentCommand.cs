using System;
using MediatR;

namespace POS.Application.Commands.Payment.Delete;

public record DeletePaymentCommand(Guid Id) : IRequest;
