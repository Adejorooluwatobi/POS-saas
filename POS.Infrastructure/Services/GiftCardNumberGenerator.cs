using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Infrastructure.Services;

public class GiftCardNumberGenerator : IGiftCardNumberGenerator
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IGiftCardRepository _giftCardRepository;

    public GiftCardNumberGenerator(
        ITenantRepository tenantRepository,
        IGiftCardRepository giftCardRepository)
    {
        _tenantRepository = tenantRepository;
        _giftCardRepository = giftCardRepository;
    }

    public async Task<string> GenerateCardNumberAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        var prefix = GetTenantPrefix(tenant);

        string cardNumber;
        bool exists;
        do
        {
            var digitsCount = 16 - prefix.Length;
            var digits = new char[digitsCount];
            for (int i = 0; i < digitsCount; i++)
            {
                digits[i] = (char)('0' + Random.Shared.Next(0, 10));
            }
            cardNumber = prefix + new string(digits);
            
            // Check uniqueness for this tenant context
            var card = await _giftCardRepository.GetByCardNumberAsync(tenantId, cardNumber);
            exists = card != null;
        } while (exists);

        return cardNumber;
    }

    private string GetTenantPrefix(Tenant? tenant)
    {
        if (tenant == null) return "GFT";

        var name = (tenant.BusinessName ?? tenant.Slug ?? "").Trim().ToLowerInvariant();
        if (name.Contains("nevermind")) return "NVMD";
        if (name.Contains("shoprite")) return "SPR";

        // Extract consonants (excluding a, e, i, o, u)
        var consonants = name.Where(c => char.IsLetter(c) && !"aeiou".Contains(c)).ToArray();
        if (consonants.Length >= 3)
        {
            var candidate = new string(consonants).ToUpperInvariant();
            return candidate.Length > 4 ? candidate.Substring(0, 4) : candidate;
        }

        // Fallback: take first 3 or 4 letters
        var cleanName = new string(name.Where(char.IsLetter).ToArray()).ToUpperInvariant();
        if (cleanName.Length >= 3)
        {
            return cleanName.Length > 4 ? cleanName.Substring(0, 4) : cleanName;
        }

        return "GFT";
    }
}
