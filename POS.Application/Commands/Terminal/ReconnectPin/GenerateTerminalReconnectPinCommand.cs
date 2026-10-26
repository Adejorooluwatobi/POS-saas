using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Terminal.ReconnectPin;

public record ReconnectPinResponse(string Pin, DateTimeOffset ExpiresAt, TerminalDto Terminal);

public record GenerateTerminalReconnectPinCommand(Guid TerminalId) : IRequest<ReconnectPinResponse>;
