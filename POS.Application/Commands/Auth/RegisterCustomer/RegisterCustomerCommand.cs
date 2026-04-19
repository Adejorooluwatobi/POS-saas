using MediatR;
using POS.Application.DTOs.Auth;

namespace POS.Application.Commands.Auth.RegisterCustomer;

public record RegisterCustomerCommand(RegisterCustomerDto Dto) : IRequest<AuthResponseDto>;
