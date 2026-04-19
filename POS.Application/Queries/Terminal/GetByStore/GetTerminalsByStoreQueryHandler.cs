using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Terminal.GetByStore;

public class GetTerminalsByStoreQueryHandler : IRequestHandler<GetTerminalsByStoreQuery, IEnumerable<TerminalDto>>
{
    private readonly ITerminalRepository _repository;
    private readonly IMapper _mapper;

    public GetTerminalsByStoreQueryHandler(ITerminalRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TerminalDto>> Handle(GetTerminalsByStoreQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetByStoreAsync(request.StoreId);
        return _mapper.Map<IEnumerable<TerminalDto>>(entities);
    }
}
