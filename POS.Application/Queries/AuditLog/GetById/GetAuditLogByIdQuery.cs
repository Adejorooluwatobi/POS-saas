using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.AuditLog.GetById;

public record GetAuditLogByIdQuery(Guid Id) : IRequest<AuditLogDto?>;

public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, AuditLogDto?>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;

    public GetAuditLogByIdQueryHandler(IAuditLogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AuditLogDto?> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(request.Id);
        return entity is null ? null : _mapper.Map<AuditLogDto>(entity);
    }
}
