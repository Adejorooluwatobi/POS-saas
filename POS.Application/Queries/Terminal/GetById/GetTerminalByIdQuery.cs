using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Terminal.GetById;

public record GetTerminalByIdQuery(Guid Id) : IRequest<TerminalDto?>;
