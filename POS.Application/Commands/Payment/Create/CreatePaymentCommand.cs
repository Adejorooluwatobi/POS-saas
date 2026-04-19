using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Payment.Create;

public record CreatePaymentCommand(CreatePaymentDto Dto) : IRequest<PaymentDto>;
