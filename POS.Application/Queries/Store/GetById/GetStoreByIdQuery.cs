using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Store.GetById;

public record GetStoreByIdQuery(Guid Id) : IRequest<StoreDto?>;
