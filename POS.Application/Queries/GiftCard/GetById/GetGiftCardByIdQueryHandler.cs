using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.GiftCard.GetById;

public class GetGiftCardByIdQueryHandler : IRequestHandler<GetGiftCardByIdQuery, GiftCardDto?>
{
    private readonly IGiftCardRepository _repository;
    private readonly IMapper _mapper;

    public GetGiftCardByIdQueryHandler(IGiftCardRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<GiftCardDto?> Handle(GetGiftCardByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        return entity != null ? _mapper.Map<GiftCardDto>(entity) : null;
    }
}
