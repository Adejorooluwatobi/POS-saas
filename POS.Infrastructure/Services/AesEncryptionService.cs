using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using POS.Domain.Interfaces;

namespace POS.Infrastructure.Services;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var configuredKey = configuration["Encryption:Key"];
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            _key = SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
        }
        else
        {
            // Default deterministic fallback key for development / tenant default
            _key = SHA256.HashData(Encoding.UTF8.GetBytes("POS_SAAS_SECURE_ENCRYPTION_MASTER_KEY_2026"));
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        
        // Write IV first
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs, Encoding.UTF8))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;

            var iv = new byte[aes.BlockSize / 8];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            return sr.ReadToEnd();
        }
        catch
        {
            // If decryption fails, return masked/empty
            return string.Empty;
        }
    }

    public string Mask(string? plainText, int visibleChars = 4)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return string.Empty;

        var trimmed = plainText.Trim();
        if (trimmed.Length <= visibleChars)
            return new string('*', trimmed.Length);

        var maskedPart = new string('*', trimmed.Length - visibleChars);
        var visiblePart = trimmed[^visibleChars..];
        return $"{maskedPart}{visiblePart}";
    }
}
