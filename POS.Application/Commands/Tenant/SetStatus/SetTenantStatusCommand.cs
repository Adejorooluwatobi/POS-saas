using MediatR;

namespace POS.Application.Commands.Tenant.SetStatus;

public record SetTenantStatusCommand(Guid Id, bool IsActive) : IRequest;
