using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.GiftCard.Delete;

public class DeleteGiftCardCommandHandler : IRequestHandler<DeleteGiftCardCommand>
{
    private readonly IGiftCardRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteGiftCardCommandHandler(IGiftCardRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteGiftCardCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"GiftCard {request.Id} not found.");

        _repository.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
