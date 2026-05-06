using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class TillSession : BaseEntity
{
    public required Guid TerminalId { get; set; }
    public required Guid StaffId { get; set; }
    public DateTimeOffset OpenedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal OpeningFloat { get; set; } = 0m;
    public decimal? ClosingCash { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? Variance { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Open;
    public string? Notes { get; set; }

    // Navigation
    public Terminal Terminal { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];
}
