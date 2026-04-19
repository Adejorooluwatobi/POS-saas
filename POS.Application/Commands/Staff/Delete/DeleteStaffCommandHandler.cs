using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Staff.Delete;

public class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand>
{
    private readonly IStaffRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteStaffCommandHandler(IStaffRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Staff {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
