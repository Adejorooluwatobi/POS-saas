using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Terminal.Pair;

public record PairTerminalCommand(string PairingCode) : IRequest<PairTerminalResponse>;

public class PairTerminalResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? DeviceToken { get; set; }
    public TerminalDto? Terminal { get; set; }
}
