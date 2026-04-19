using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Role.GetById;

public record GetRoleByIdQuery(Guid Id) : IRequest<RoleDto?>;
