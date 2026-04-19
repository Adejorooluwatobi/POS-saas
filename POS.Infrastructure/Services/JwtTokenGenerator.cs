using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using POS.Domain.Interfaces;

namespace POS.Infrastructure.Services;

public class JwtTokenGenerator : ITokenService
{
    private readonly IConfiguration _config;
    public JwtTokenGenerator(IConfiguration config) => _config = config;

    public string GenerateToken(Guid id, string emailOrEmployeeNo, string role, Guid? tenantId = null, Guid? storeId = null)
    {
        var secret = _config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret missing.");
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var expMin = int.TryParse(_config["Jwt:ExpirationInMinutes"], out var e) ? e : 1440;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, id.ToString()),
            new("system_role", role)
        };

        if (emailOrEmployeeNo.Contains('@'))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, emailOrEmployeeNo));
            claims.Add(new Claim(ClaimTypes.Email, emailOrEmployeeNo));
        }
        else
        {
            claims.Add(new Claim("employee_no", emailOrEmployeeNo));
        }

        if (tenantId.HasValue)
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));

        if (storeId.HasValue)
            claims.Add(new Claim("store_id", storeId.Value.ToString()));

        // We also add role using standard claim for [Authorize(Roles="...")]
        claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expMin),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
