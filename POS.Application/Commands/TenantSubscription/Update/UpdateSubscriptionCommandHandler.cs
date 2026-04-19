using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.TenantSubscription.Update;

public class UpdateSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, TenantSubscriptionDto>
{
    private readonly ITenantSubscriptionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateSubscriptionCommandHandler(ITenantSubscriptionRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<TenantSubscriptionDto> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByTenantAsync(request.TenantId)
            ?? throw new KeyNotFoundException($"No subscription found for tenant {request.TenantId}.");

        _mapper.Map(request.Dto, entity);
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TenantSubscriptionDto>(entity);
    }
}
