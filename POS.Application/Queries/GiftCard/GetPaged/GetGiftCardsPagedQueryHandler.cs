using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.GiftCard.GetPaged;

public class GetGiftCardsPagedQueryHandler : IRequestHandler<GetGiftCardsPagedQuery, PagedResult<GiftCardDto>>
{
    private readonly IGiftCardRepository _repository;
    private readonly IMapper _mapper;

    public GetGiftCardsPagedQueryHandler(IGiftCardRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<GiftCardDto>> Handle(GetGiftCardsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<GiftCardDto>
        {
            Items = _mapper.Map<IEnumerable<GiftCardDto>>(pagedResult.Items),
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }
}
