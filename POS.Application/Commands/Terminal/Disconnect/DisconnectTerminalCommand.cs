using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Terminal.Disconnect;

public record DisconnectTerminalCommand(Guid TerminalId) : IRequest<TerminalDto>;
