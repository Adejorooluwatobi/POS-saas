using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Loyalty.UpdateLoyaltySettings;

public record UpdateLoyaltySettingsCommand(LoyaltySettingsDto Dto) : IRequest<LoyaltySettingsDto>;
