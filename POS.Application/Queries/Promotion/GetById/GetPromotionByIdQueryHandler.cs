using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Promotion.GetById;

public class GetPromotionByIdQueryHandler : IRequestHandler<GetPromotionByIdQuery, PromotionDto?>
{
    private readonly IPromotionRepository _repository;
    private readonly IMapper _mapper;

    public GetPromotionByIdQueryHandler(IPromotionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PromotionDto?> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        return entity != null ? _mapper.Map<PromotionDto>(entity) : null;
    }
}
