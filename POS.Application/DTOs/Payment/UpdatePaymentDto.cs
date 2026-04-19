using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class UpdatePaymentDto
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public string? GatewayRef { get; set; }
}
