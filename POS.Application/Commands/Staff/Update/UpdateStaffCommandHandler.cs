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
    private readonly IPasswordService _passwordService;

    public UpdateStaffCommandHandler(IStaffRepository repository, IUnitOfWork uow, IMapper mapper, IPasswordService passwordService)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _passwordService = passwordService;
    }

    public async Task Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Staff {request.Id} not found.");

        _mapper.Map(request.Dto, entity);

        if (!string.IsNullOrWhiteSpace(request.Dto.Pin))
        {
            entity.PinHash = _passwordService.Hash(request.Dto.Pin);
        }

        if (request.Dto.Password != null)
        {
            if (string.IsNullOrWhiteSpace(request.Dto.Password))
            {
                entity.PasswordHash = null;
            }
            else
            {
                entity.PasswordHash = _passwordService.Hash(request.Dto.Password);
            }
        }
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
