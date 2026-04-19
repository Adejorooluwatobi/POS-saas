using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Role.Create;

public record CreateRoleCommand(CreateRoleDto Dto) : IRequest<RoleDto>;
