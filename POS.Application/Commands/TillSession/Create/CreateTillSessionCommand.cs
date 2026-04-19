using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.TillSession.Create;

public record CreateTillSessionCommand(CreateTillSessionDto Dto) : IRequest<TillSessionDto>;
