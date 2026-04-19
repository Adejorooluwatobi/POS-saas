using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Terminal.Update;

public record UpdateTerminalCommand(Guid Id, UpdateTerminalDto Dto) : IRequest;
