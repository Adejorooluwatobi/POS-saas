namespace POS.Domain.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string Mask(string? plainText, int visibleChars = 4);
}
