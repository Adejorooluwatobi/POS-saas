using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Store.Update;

public record UpdateStoreCommand(Guid Id, UpdateStoreDto Dto) : IRequest;
