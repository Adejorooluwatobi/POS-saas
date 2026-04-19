using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Tenant.GetById;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly ITenantRepository _repository;
    private readonly IMapper _mapper;

    public GetTenantByIdQueryHandler(ITenantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        return entity != null ? _mapper.Map<TenantDto>(entity) : null;
    }
}
