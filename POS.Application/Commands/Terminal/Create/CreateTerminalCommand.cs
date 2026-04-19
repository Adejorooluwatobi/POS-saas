using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Terminal.Create;

public record CreateTerminalCommand(Guid StoreId, CreateTerminalDto Dto) : IRequest<TerminalDto>;
