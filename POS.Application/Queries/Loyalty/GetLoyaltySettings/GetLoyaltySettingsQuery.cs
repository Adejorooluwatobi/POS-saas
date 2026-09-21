using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Loyalty.GetLoyaltySettings;

public record GetLoyaltySettingsQuery : IRequest<LoyaltySettingsDto>;
