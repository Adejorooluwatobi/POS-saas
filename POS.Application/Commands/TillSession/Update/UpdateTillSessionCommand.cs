using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.TillSession.Update;

public record UpdateTillSessionCommand(Guid Id, UpdateTillSessionDto Dto) : IRequest;
