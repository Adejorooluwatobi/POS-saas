using System;

namespace POS.Application.DTOs.Auth;

/// <summary>Used when onboarding a brand new Business to the platform.</summary>
public record RegisterTenantDto(
    string BusinessName,
    string Slug,
    string ContactEmail,
    string Country,

    string OwnerFirstName,
    string OwnerLastName,
    string OwnerEmail,
    string OwnerPassword
);

/// <summary>Used when a consumer signs up for loyalty / digital receipts.</summary>
public record RegisterCustomerDto(
    Guid TenantId,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone
);
