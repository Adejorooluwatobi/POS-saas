namespace POS.Domain.Interfaces;

public interface IPasswordService
{
    string Hash(string plainText);
    bool Verify(string plainText, string hash);
    string GenerateRandomPassword(int length = 10);
    string GenerateRandomPin(int length = 4);
}
