using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class Terminal : BaseEntity
{
    public required Guid StoreId { get; set; }
    public required string TerminalCode { get; set; }
    public string? Label { get; set; }
    public string? IpAddress { get; set; }
    public TerminalStatus Status { get; set; } = TerminalStatus.Offline;
    public DateTimeOffset? LastPingAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<TillSession> TillSessions { get; set; } = [];
}
