using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using POS.Domain.Interfaces;

namespace POS.Application.Queries.Customer.GenerateLoyaltyNumber;

public class GenerateLoyaltyNumberQueryHandler : IRequestHandler<GenerateLoyaltyNumberQuery, string>
{
    private readonly ILoyaltyCardNumberGenerator _generator;
    private readonly ITenantContext _tenantContext;

    public GenerateLoyaltyNumberQueryHandler(
        ILoyaltyCardNumberGenerator generator,
        ITenantContext tenantContext)
    {
        _generator = generator;
        _tenantContext = tenantContext;
    }

    public async Task<string> Handle(GenerateLoyaltyNumberQuery request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("Tenant context is required to generate a loyalty card number.");

        var storeId = request.StoreId ?? _tenantContext.StoreId;
        return await _generator.GenerateLoyaltyCardNumberAsync(_tenantContext.TenantId.Value, storeId, cancellationToken);
    }
}
