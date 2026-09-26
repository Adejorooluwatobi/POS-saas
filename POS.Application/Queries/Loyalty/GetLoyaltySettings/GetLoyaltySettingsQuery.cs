using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Loyalty.GetLoyaltySettings;

public record GetLoyaltySettingsQuery(Guid? TenantId = null) : IRequest<LoyaltySettingsDto>;
