using MediatR;
using POS.Application.DTOs.Auth;

namespace POS.Application.Commands.Auth.LoginCustomer;

public record LoginCustomerCommand(CustomerLoginDto Dto) : IRequest<AuthResponseDto>;
