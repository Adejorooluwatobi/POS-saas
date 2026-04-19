using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class CreatePaymentDto
{
    public Guid TransactionId { get; set; }
    public Guid? GiftCardId { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public decimal? AmountTendered { get; set; }
}
