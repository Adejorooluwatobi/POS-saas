namespace POS.Domain.Interfaces;

public interface IReceiptNumberService
{
    string Generate(string storeCode);
}
