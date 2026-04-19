using MediatR;

namespace POS.Application.Commands.Terminal.Delete;

public record DeleteTerminalCommand(Guid Id) : IRequest;
