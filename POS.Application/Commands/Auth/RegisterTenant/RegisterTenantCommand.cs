using MediatR;
using POS.Application.DTOs.Auth;

namespace POS.Application.Commands.Auth.RegisterTenant;

public record RegisterTenantCommand(RegisterTenantDto Dto) : IRequest<AuthResponseDto>;
