namespace POS.Application.DTOs.Auth;

public record AuthResponseDto(string Token, string Role, Guid? TenantId, string Name, Guid UserId, string Email, Guid? StoreId, string? BusinessName = null);
