using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Loyalty.UpdateLoyaltySettings;

public class UpdateLoyaltySettingsCommandHandler : IRequestHandler<UpdateLoyaltySettingsCommand, LoyaltySettingsDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public UpdateLoyaltySettingsCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork uow,
        ITenantContext tenantContext)
    {
        _tenantRepository = tenantRepository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task<LoyaltySettingsDto> Handle(UpdateLoyaltySettingsCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("No tenant context is available.");

        var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId.Value)
            ?? throw new KeyNotFoundException("Tenant not found.");

        if (request.Dto.LoyaltyPointsEarnRate <= 0)
            throw new InvalidOperationException("Points earn rate must be greater than zero.");

        if (request.Dto.LoyaltyPointRedeemRate <= 0)
            throw new InvalidOperationException("Point redeem rate must be greater than zero.");

        tenant.LoyaltyProgramEnabled = request.Dto.LoyaltyProgramEnabled;
        tenant.LoyaltyPointsEarnRate = request.Dto.LoyaltyPointsEarnRate;
        tenant.LoyaltyPointRedeemRate = request.Dto.LoyaltyPointRedeemRate;
        tenant.LoyaltyMinRedemptionPoints = Math.Max(0, request.Dto.LoyaltyMinRedemptionPoints);

        _tenantRepository.Update(tenant);
        await _uow.SaveChangesAsync(cancellationToken);

        return new LoyaltySettingsDto
        {
            LoyaltyProgramEnabled = tenant.LoyaltyProgramEnabled,
            LoyaltyPointsEarnRate = tenant.LoyaltyPointsEarnRate,
            LoyaltyPointRedeemRate = tenant.LoyaltyPointRedeemRate,
            LoyaltyMinRedemptionPoints = tenant.LoyaltyMinRedemptionPoints
        };
    }
}
