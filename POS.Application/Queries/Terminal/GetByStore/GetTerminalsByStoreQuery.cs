using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Terminal.GetByStore;

public record GetTerminalsByStoreQuery(Guid StoreId) : IRequest<IEnumerable<TerminalDto>>;
