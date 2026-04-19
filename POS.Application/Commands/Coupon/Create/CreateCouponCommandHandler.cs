using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Coupon;

namespace POS.Application.Commands.Coupon.Create;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CouponDto>
{
    private readonly ICouponRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateCouponCommandHandler(ICouponRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);
        entity.PromotionId = request.PromotionId;
        entity.IsActive = true;
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CouponDto>(entity);
    }
}
