namespace POS.Application.DTOs.Auth;

/// <summary>Used by Super Admins and Tenant Admins to log into the web dashboard.</summary>
public record AdminLoginDto(string Email, string Password);

/// <summary>Used by Cashiers to log into physical Store Terminals quickly.</summary>
public record PosLoginDto(Guid StoreId, string EmployeeNo, string Pin);

/// <summary>Used by Consumers logging into a loyalty website/app.</summary>
public record CustomerLoginDto(string EmailOrPhone, string Password);
