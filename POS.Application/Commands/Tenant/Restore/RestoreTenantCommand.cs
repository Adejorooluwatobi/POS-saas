using System;
using MediatR;

namespace POS.Application.Commands.Tenant.Restore;

public record RestoreTenantCommand(Guid Id) : IRequest;
