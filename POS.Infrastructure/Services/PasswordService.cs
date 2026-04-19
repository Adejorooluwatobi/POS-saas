using Microsoft.AspNetCore.Identity;
using POS.Domain.Interfaces;

namespace POS.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string plainText) =>
        _hasher.HashPassword(null!, plainText);

    public bool Verify(string plainText, string hash) =>
        _hasher.VerifyHashedPassword(null!, hash, plainText) != PasswordVerificationResult.Failed;
}
