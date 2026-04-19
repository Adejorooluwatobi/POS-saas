using System;
using MediatR;

namespace POS.Application.Commands.Store.Delete;

public record DeleteStoreCommand(Guid Id) : IRequest;
