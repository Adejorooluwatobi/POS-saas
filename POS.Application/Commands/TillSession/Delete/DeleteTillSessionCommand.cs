using System;
using MediatR;

namespace POS.Application.Commands.TillSession.Delete;

public record DeleteTillSessionCommand(Guid Id) : IRequest;
