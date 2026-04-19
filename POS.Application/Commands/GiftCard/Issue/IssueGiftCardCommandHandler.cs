using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.GiftCard;

namespace POS.Application.Commands.GiftCard.Issue;

public class IssueGiftCardCommandHandler : IRequestHandler<IssueGiftCardCommand, GiftCardDto>
{
    private readonly IGiftCardRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public IssueGiftCardCommandHandler(
        IGiftCardRepository repository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<GiftCardDto> Handle(IssueGiftCardCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("No tenant context is available.");

        var entity = _mapper.Map<Entity>(request.Dto);
        entity.TenantId = _tenantContext.TenantId.Value;
        entity.Balance = request.Dto.InitialValue;
        entity.IsActive = true;
        entity.IssuedAt = DateTimeOffset.UtcNow;
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<GiftCardDto>(entity);
    }
}
