using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Tenant.Purge;

public class PurgeTenantCommandHandler : IRequestHandler<PurgeTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;

    public PurgeTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task Handle(PurgeTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {request.Id} not found.");

        if (tenant.Slug.Equals("system", StringComparison.OrdinalIgnoreCase) ||
            tenant.Id == Guid.Parse("00000000-0000-0000-0000-000000000001"))
        {
            throw new InvalidOperationException("The system tenant cannot be permanently purged.");
        }

        await _tenantRepository.PurgeTenantAsync(request.Id, cancellationToken);
    }
}
