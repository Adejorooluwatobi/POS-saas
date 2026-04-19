using System;
using MediatR;

namespace POS.Application.Commands.Tenant.Delete;

public record DeleteTenantCommand(Guid Id) : IRequest;
