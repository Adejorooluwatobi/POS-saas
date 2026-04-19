using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Tenant.Create;

public record CreateTenantCommand(CreateTenantDto Dto) : IRequest<TenantDto>;
