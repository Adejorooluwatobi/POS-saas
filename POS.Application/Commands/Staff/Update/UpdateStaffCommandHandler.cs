using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Staff.Update;

public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand>
{
    private readonly IStaffRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateStaffCommandHandler(IStaffRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Staff {request.Id} not found.");

        _mapper.Map(request.Dto, entity);
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
