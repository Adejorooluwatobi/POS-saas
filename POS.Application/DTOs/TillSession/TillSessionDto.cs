using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class TillSessionDto
{
    public Guid Id { get; set; }
    public Guid TerminalId { get; set; }
    public Guid StaffId { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? Variance { get; set; }
    public SessionStatus Status { get; set; }
    public string? Notes { get; set; }
}
