using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.TillSession.GetById;

public class GetTillSessionByIdQueryHandler : IRequestHandler<GetTillSessionByIdQuery, TillSessionDto?>
{
    private readonly ITillSessionRepository _repository;
    private readonly IMapper _mapper;

    public GetTillSessionByIdQueryHandler(ITillSessionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TillSessionDto?> Handle(GetTillSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        return entity != null ? _mapper.Map<TillSessionDto>(entity) : null;
    }
}
