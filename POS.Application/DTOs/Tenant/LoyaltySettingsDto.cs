namespace POS.Application.DTOs;

public class LoyaltySettingsDto
{
    public bool LoyaltyProgramEnabled { get; set; } = true;
    public decimal LoyaltyPointsEarnRate { get; set; } = 100m;
    public decimal LoyaltyPointRedeemRate { get; set; } = 1m;
    public int LoyaltyMinRedemptionPoints { get; set; } = 50;
}
