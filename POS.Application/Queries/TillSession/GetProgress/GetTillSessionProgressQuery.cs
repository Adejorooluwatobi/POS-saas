using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.TillSession.GetProgress;

public record GetTillSessionProgressQuery(Guid SessionId) : IRequest<TillSessionProgressDto>;
