using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Promotion.Delete;

public class DeletePromotionCommandHandler : IRequestHandler<DeletePromotionCommand>
{
    private readonly IPromotionRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeletePromotionCommandHandler(IPromotionRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Promotion {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
