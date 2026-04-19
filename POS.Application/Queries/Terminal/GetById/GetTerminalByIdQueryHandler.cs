using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Terminal.GetById;

public class GetTerminalByIdQueryHandler : IRequestHandler<GetTerminalByIdQuery, TerminalDto?>
{
    private readonly ITerminalRepository _repository;
    private readonly IMapper _mapper;

    public GetTerminalByIdQueryHandler(ITerminalRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TerminalDto?> Handle(GetTerminalByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        return entity != null ? _mapper.Map<TerminalDto>(entity) : null;
    }
}
