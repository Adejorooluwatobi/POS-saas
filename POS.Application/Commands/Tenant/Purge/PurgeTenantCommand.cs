using System;
using MediatR;

namespace POS.Application.Commands.Tenant.Purge;

public record PurgeTenantCommand(Guid Id) : IRequest;
