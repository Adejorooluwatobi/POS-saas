using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Payment.Delete;

public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand>
{
    private readonly IPaymentRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeletePaymentCommandHandler(IPaymentRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Payment {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
