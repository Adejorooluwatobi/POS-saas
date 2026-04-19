using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Customer;

namespace POS.Application.Commands.Customer.Create;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public CreateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork uow, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);
        entity.TenantId = _tenantContext.TenantId!.Value;

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CustomerDto>(entity);
    }
}
