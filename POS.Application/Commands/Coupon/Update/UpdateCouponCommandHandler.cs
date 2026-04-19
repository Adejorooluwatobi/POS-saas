using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Coupon.Update;

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand>
{
    private readonly ICouponRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateCouponCommandHandler(ICouponRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Coupon {request.Id} not found.");

        _mapper.Map(request.Dto, entity);
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
