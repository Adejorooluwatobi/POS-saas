using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.GiftCard.Update;

public class UpdateGiftCardCommandHandler : IRequestHandler<UpdateGiftCardCommand>
{
    private readonly IGiftCardRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdateGiftCardCommandHandler(IGiftCardRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(UpdateGiftCardCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"GiftCard {request.Id} not found.");

        entity.IsActive = request.Dto.IsActive;
        if (request.Dto.ExpiresAt.HasValue)
        {
            entity.ExpiresAt = request.Dto.ExpiresAt;
        }

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
