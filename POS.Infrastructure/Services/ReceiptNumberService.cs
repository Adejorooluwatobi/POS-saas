using POS.Domain.Interfaces;

namespace POS.Infrastructure.Services;

public class ReceiptNumberService : IReceiptNumberService
{
    public string Generate(string? storeCode)
    {
        var safeCode = string.IsNullOrWhiteSpace(storeCode) ? "STR" : storeCode.ToUpper();
        var datePart = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var random = Random.Shared.Next(1000, 9999);
        return $"{safeCode}-{datePart}-{random}";
    }
}
