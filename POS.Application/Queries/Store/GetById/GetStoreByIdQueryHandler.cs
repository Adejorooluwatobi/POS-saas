using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Store.GetById;

public class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, StoreDto?>
{
    private readonly IStoreRepository _repository;
    private readonly IMapper _mapper;

    public GetStoreByIdQueryHandler(IStoreRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<StoreDto?> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        return entity != null ? _mapper.Map<StoreDto>(entity) : null;
    }
}
