using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Loyalty.GetLoyaltySettings;

public class GetLoyaltySettingsQueryHandler : IRequestHandler<GetLoyaltySettingsQuery, LoyaltySettingsDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantContext _tenantContext;

    public GetLoyaltySettingsQueryHandler(ITenantRepository tenantRepository, ITenantContext tenantContext)
    {
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
    }

    public async Task<LoyaltySettingsDto> Handle(GetLoyaltySettingsQuery request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("No tenant context is available.");

        var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId.Value);
        if (tenant is null)
        {
            return new LoyaltySettingsDto();
        }

        return new LoyaltySettingsDto
        {
            LoyaltyProgramEnabled = tenant.LoyaltyProgramEnabled,
            LoyaltyPointsEarnRate = tenant.LoyaltyPointsEarnRate,
            LoyaltyPointRedeemRate = tenant.LoyaltyPointRedeemRate,
            LoyaltyMinRedemptionPoints = tenant.LoyaltyMinRedemptionPoints
        };
    }
}
