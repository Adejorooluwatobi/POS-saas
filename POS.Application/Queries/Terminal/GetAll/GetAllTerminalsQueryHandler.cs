using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Terminal.GetAll;

public class GetAllTerminalsQueryHandler : IRequestHandler<GetAllTerminalsQuery, PagedResult<TerminalDto>>
{
    private readonly ITerminalRepository _repository;
    private readonly IMapper _mapper;

    public GetAllTerminalsQueryHandler(ITerminalRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TerminalDto>> Handle(GetAllTerminalsQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);

        return new PagedResult<TerminalDto>
        {
            Items = _mapper.Map<IEnumerable<TerminalDto>>(pagedResult.Items),
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }
}
