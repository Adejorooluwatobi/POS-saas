using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Coupon.GetByPromotion;

public class GetCouponsByPromotionQueryHandler : IRequestHandler<GetCouponsByPromotionQuery, IEnumerable<CouponDto>>
{
    private readonly ICouponRepository _repository;
    private readonly IMapper _mapper;

    public GetCouponsByPromotionQueryHandler(ICouponRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CouponDto>> Handle(GetCouponsByPromotionQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetByPromotionAsync(request.PromotionId);
        return _mapper.Map<IEnumerable<CouponDto>>(entities);
    }
}
