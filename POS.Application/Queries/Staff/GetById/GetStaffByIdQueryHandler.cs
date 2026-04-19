using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Staff.GetById;

public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, StaffDto?>
{
    private readonly IStaffRepository _repository;
    private readonly IMapper _mapper;

    public GetStaffByIdQueryHandler(IStaffRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<StaffDto?> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        return entity != null ? _mapper.Map<StaffDto>(entity) : null;
    }
}
