using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Customer.Delete;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Customer {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
