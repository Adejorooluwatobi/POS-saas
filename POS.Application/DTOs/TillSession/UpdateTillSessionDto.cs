using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class UpdateTillSessionDto
{
    public Guid Id { get; set; }
    public decimal? ClosingCash { get; set; }
    public SessionStatus Status { get; set; }
    public string? Notes { get; set; }
}
