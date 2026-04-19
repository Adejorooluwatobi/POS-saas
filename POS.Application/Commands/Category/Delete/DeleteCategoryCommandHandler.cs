using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Category.Delete;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteCategoryCommandHandler(ICategoryRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Category {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
