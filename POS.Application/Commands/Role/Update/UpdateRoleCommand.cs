using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Role.Update;

public record UpdateRoleCommand(Guid Id, UpdateRoleDto Dto) : IRequest;
