using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Category.Update;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateCategoryCommandHandler(ICategoryRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Category {request.Id} not found.");

        _mapper.Map(request.Dto, entity);
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
