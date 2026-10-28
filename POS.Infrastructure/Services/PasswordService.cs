using System;
using System.Security.Cryptography;
using System.Text;
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

    public string GenerateRandomPassword(int length = 10)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // exclude easily confused I, O
        const string lower = "abcdefghijkmnopqrstuvwxyz"; // exclude l
        const string digits = "23456789";                  // exclude 0, 1
        const string special = "!@#$%*";
        const string all = upper + lower + digits + special;

        var sb = new StringBuilder();
        // Ensure at least one from each character group
        sb.Append(upper[RandomNumberGenerator.GetInt32(upper.Length)]);
        sb.Append(lower[RandomNumberGenerator.GetInt32(lower.Length)]);
        sb.Append(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
        sb.Append(special[RandomNumberGenerator.GetInt32(special.Length)]);

        for (int i = 4; i < length; i++)
        {
            sb.Append(all[RandomNumberGenerator.GetInt32(all.Length)]);
        }

        // Shuffle characters
        var array = sb.ToString().ToCharArray();
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }

        return new string(array);
    }

    public string GenerateRandomPin(int length = 4)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(RandomNumberGenerator.GetInt32(10));
        }
        return sb.ToString();
    }
}
