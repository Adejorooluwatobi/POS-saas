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
        var entity = await _repository.GetByTenantAsync(request.TenantId);

        if (entity == null)
        {
            // Create a subscription if none exists (e.g., for tenants onboarded before this feature)
            entity = new POS.Domain.Entities.TenantSubscription
            {
                TenantId = request.TenantId,
                Plan = request.Dto.Plan,
                Status = request.Dto.Status,
                BillingCycle = request.Dto.BillingCycle,
                MaxStores = request.Dto.MaxStores > 0 ? request.Dto.MaxStores : 1,
                MaxStaff = request.Dto.MaxStaff > 0 ? request.Dto.MaxStaff : 5,
                MaxTerminals = request.Dto.MaxTerminals > 0 ? request.Dto.MaxTerminals : 2,
                MonthlyPrice = request.Dto.MonthlyPrice,
                CurrentPeriodStart = DateTimeOffset.UtcNow,
                CurrentPeriodEnd = request.Dto.CurrentPeriodEnd != default
                    ? request.Dto.CurrentPeriodEnd
                    : DateTimeOffset.UtcNow.AddYears(1)
            };
            await _repository.AddAsync(entity);
        }
        else
        {
            _mapper.Map(request.Dto, entity);
            _repository.Update(entity);
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TenantSubscriptionDto>(entity);
    }
}
