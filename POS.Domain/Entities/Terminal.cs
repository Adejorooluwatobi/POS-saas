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
    
    // Device Pairing & 30-Day Lease
    public string? PairingCode { get; set; }
    public string? DeviceToken { get; set; }
    public DateTimeOffset? PairingCodeExpiresAt { get; set; }
    public DateTimeOffset? LeaseExpiresAt { get; set; }
    public DateTimeOffset? DisconnectedAt { get; set; }
    public string? DisconnectedBy { get; set; }

    public bool IsLeaseActive => Status != TerminalStatus.Disconnected 
                              && !string.IsNullOrEmpty(DeviceToken) 
                              && LeaseExpiresAt.HasValue 
                              && LeaseExpiresAt.Value > DateTimeOffset.UtcNow;

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<TillSession> TillSessions { get; set; } = [];
}
