using System;
using System.Threading;
using System.Threading.Tasks;

namespace POS.Domain.Interfaces;

public interface IGiftCardNumberGenerator
{
    Task<string> GenerateCardNumberAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
