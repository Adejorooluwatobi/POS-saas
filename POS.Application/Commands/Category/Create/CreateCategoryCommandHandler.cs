using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Category;

namespace POS.Application.Commands.Category.Create;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public CreateCategoryCommandHandler(ICategoryRepository repository, IUnitOfWork uow, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? request.Dto.TenantId;
        
        if (tenantId == null || tenantId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Tenant context is missing. SuperAdmins must provide a TenantId.");
        }

        var entity = _mapper.Map<Entity>(request.Dto);
        entity.TenantId = tenantId.Value;
        
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CategoryDto>(entity);
    }
}
