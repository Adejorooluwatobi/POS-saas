namespace POS.Application.DTOs.Auth;

public record AuthResponseDto(string Token, string Role, Guid? TenantId, string Name);
