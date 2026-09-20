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
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            // Derive a deterministic 256-bit key using a default platform secret
            configuredKey = "POS_SAAS_SECURE_DEFAULT_AES_KEY_2026";
        }

        using var sha = SHA256.Create();
        _key = sha.ComputeHash(Encoding.UTF8.GetBytes(configuredKey));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

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
            return cipherText;

        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            if (fullCipher.Length < 16)
                return cipherText; // Return original if not a valid cipher

            using var aes = Aes.Create();
            aes.Key = _key;

            var iv = new byte[16];
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
            // Graceful fallback if cipher format is invalid or legacy
            return cipherText;
        }
    }

    public string Mask(string plainText, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        var clean = plainText.Trim();
        if (clean.Length <= visibleChars)
            return new string('*', clean.Length);

        var maskCount = clean.Length - visibleChars;
        var suffix = clean.Substring(maskCount);
        return new string('*', maskCount) + suffix;
    }
}
