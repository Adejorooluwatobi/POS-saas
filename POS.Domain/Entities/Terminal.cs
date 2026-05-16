using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

[Auditable]
public class Terminal : AuditableEntity
{
    public required Guid StoreId { get; set; }
    public required string TerminalCode { get; set; }
    public string? Label { get; set; }
    public string? IpAddress { get; set; }
    public TerminalStatus Status { get; set; } = TerminalStatus.Offline;
    public DateTimeOffset? LastPingAt { get; set; }
    
    // Device Pairing
    public string? PairingCode { get; set; }
    public string? DeviceToken { get; set; }
    public DateTimeOffset? PairingCodeExpiresAt { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<TillSession> TillSessions { get; set; } = [];
}
