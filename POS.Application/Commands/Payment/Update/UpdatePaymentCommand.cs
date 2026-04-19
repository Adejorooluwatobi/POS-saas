using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Payment.Update;

public record UpdatePaymentCommand(Guid Id, UpdatePaymentDto Dto) : IRequest;
