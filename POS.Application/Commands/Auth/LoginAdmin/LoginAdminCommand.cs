using MediatR;
using POS.Application.DTOs.Auth;

namespace POS.Application.Commands.Auth.LoginAdmin;

public record LoginAdminCommand(AdminLoginDto Dto) : IRequest<AuthResponseDto>;
