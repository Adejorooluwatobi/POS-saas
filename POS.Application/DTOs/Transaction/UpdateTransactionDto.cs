using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class UpdateTransactionDto
{
    public Guid Id { get; set; }
    public TransactionStatus Status { get; set; }
    public string? Notes { get; set; }
}
