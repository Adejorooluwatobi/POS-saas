using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Promotion.Update;

public class UpdatePromotionCommandHandler : IRequestHandler<UpdatePromotionCommand>
{
    private readonly IPromotionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public UpdatePromotionCommandHandler(IPromotionRepository repository, IUnitOfWork uow, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Promotion {request.Id} not found.");

        _mapper.Map(request.Dto, entity);

        // Enforce Scoping on update
        if (_tenantContext.SystemRole is "StoreManager" or "Supervisor" or "Cashier")
        {
            entity.StoreId = _tenantContext.StoreId;
        }

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
