using System.Threading;
using System.Threading.Tasks;
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
        var tenantId = request.TenantId ?? _tenantContext.TenantId;
        if (tenantId is null)
        {
            // SuperAdmin global scope or unselected tenant: return safe defaults without throwing
            return new LoyaltySettingsDto
            {
                LoyaltyProgramEnabled = true,
                LoyaltyPointsEarnRate = 100,
                LoyaltyPointRedeemRate = 1,
                LoyaltyMinRedemptionPoints = 50
            };
        }

        var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value);
        if (tenant is null)
        {
            return new LoyaltySettingsDto
            {
                LoyaltyProgramEnabled = true,
                LoyaltyPointsEarnRate = 100,
                LoyaltyPointRedeemRate = 1,
                LoyaltyMinRedemptionPoints = 50
            };
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
