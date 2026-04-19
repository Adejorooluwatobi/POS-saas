using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Product.Delete;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteProductCommandHandler(IProductRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
