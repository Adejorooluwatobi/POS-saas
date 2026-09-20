using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Infrastructure.Services;

public class LoyaltyCardNumberGenerator : ILoyaltyCardNumberGenerator
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ICustomerRepository _customerRepository;

    public LoyaltyCardNumberGenerator(
        ITenantRepository tenantRepository,
        IStoreRepository storeRepository,
        ICustomerRepository customerRepository)
    {
        _tenantRepository = tenantRepository;
        _storeRepository = storeRepository;
        _customerRepository = customerRepository;
    }

    public async Task<string> GenerateLoyaltyCardNumberAsync(Guid tenantId, Guid? storeId = null, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        var prefix = GetPrefix(tenant);

        if (storeId.HasValue)
        {
            var store = await _storeRepository.GetByIdAsync(storeId.Value);
            if (store != null && !string.IsNullOrWhiteSpace(store.Code))
            {
                prefix = store.Code.Trim().ToUpperInvariant();
            }
        }

        string loyaltyCardNo;
        bool exists;
        do
        {
            var randomDigits = Random.Shared.Next(100000, 999999).ToString();
            loyaltyCardNo = $"{prefix}-LOY-{randomDigits}";

            exists = await _customerRepository.GetQueryable()
                .AnyAsync(c => c.TenantId == tenantId && c.LoyaltyCardNo == loyaltyCardNo, cancellationToken);
        } while (exists);

        return loyaltyCardNo;
    }

    private string GetPrefix(Tenant? tenant)
    {
        if (tenant == null) return "LOY";

        var name = (tenant.BusinessName ?? tenant.Slug ?? "").Trim().ToLowerInvariant();
        if (name.Contains("nevermind")) return "NVMD";
        if (name.Contains("shoprite")) return "SHPR";
        if (name.Contains("spar")) return "SPAR";

        var consonants = name.Where(c => char.IsLetter(c) && !"aeiou".Contains(c)).ToArray();
        if (consonants.Length >= 3)
        {
            var candidate = new string(consonants).ToUpperInvariant();
            return candidate.Length > 4 ? candidate.Substring(0, 4) : candidate;
        }

        var cleanName = new string(name.Where(char.IsLetter).ToArray()).ToUpperInvariant();
        if (cleanName.Length >= 3)
        {
            return cleanName.Length > 4 ? cleanName.Substring(0, 4) : cleanName;
        }

        return "RET";
    }
}
