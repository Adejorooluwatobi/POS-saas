using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.TillSession.GetById;

public record GetTillSessionByIdQuery(Guid Id) : IRequest<TillSessionDto?>;
