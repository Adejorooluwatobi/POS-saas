using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class TerminalDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string TerminalCode { get; set; } = default!;
    public string? Label { get; set; }
    public string? IpAddress { get; set; }
    public TerminalStatus Status { get; set; }
    public DateTimeOffset? LastPingAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateTerminalDto
{
    public string TerminalCode { get; set; } = default!;
    public string? Label { get; set; }
    public string? IpAddress { get; set; }
}

public class UpdateTerminalDto
{
    public Guid Id { get; set; }
    public string? Label { get; set; }
    public string? IpAddress { get; set; }
    public TerminalStatus Status { get; set; }
}
