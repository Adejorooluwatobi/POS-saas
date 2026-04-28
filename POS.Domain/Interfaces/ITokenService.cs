using System;
using POS.Domain.Enums;

namespace POS.Domain.Interfaces;

public interface ITokenService
{
    string GenerateToken(Guid id, string emailOrEmployeeNo, string role, string fullName, Guid? tenantId = null, Guid? storeId = null);
}
