using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Payment.GetById;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentDto?>;
