using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Store.Create;

public record CreateStoreCommand(CreateStoreDto Dto) : IRequest<StoreDto>;
