using System;
using MediatR;

namespace POS.Application.Commands.Role.Delete;

public record DeleteRoleCommand(Guid Id) : IRequest;
