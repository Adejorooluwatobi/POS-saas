using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Product.Update;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IProductRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found.");

        _mapper.Map(request.Dto, entity);
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
