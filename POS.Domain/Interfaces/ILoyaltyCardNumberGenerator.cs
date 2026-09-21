using System;
using System.Threading;
using System.Threading.Tasks;

namespace POS.Domain.Interfaces;

public interface ILoyaltyCardNumberGenerator
{
    Task<string> GenerateLoyaltyCardNumberAsync(Guid tenantId, Guid? storeId = null, CancellationToken cancellationToken = default);
}
