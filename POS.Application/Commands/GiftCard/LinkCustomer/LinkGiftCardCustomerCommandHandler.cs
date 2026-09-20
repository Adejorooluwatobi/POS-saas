using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.GiftCard.LinkCustomer;

public class LinkGiftCardCustomerCommandHandler : 
    IRequestHandler<LinkGiftCardCustomerCommand, GiftCardDto>,
    IRequestHandler<UnlinkGiftCardCustomerCommand, GiftCardDto>
{
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public LinkGiftCardCustomerCommandHandler(
        IGiftCardRepository giftCardRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _giftCardRepository = giftCardRepository;
        _customerRepository = customerRepository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<GiftCardDto> Handle(LinkGiftCardCustomerCommand request, CancellationToken cancellationToken)
    {
        var card = await _giftCardRepository.GetByIdAsync(request.GiftCardId)
            ?? throw new KeyNotFoundException($"Gift card with ID '{request.GiftCardId}' was not found.");

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId)
            ?? throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' was not found.");

        card.CustomerId = customer.Id;
        _giftCardRepository.Update(card);
        await _uow.SaveChangesAsync(cancellationToken);

        var refreshed = await _giftCardRepository.GetByIdWithDetailsAsync(card.Id);
        return _mapper.Map<GiftCardDto>(refreshed ?? card);
    }

    public async Task<GiftCardDto> Handle(UnlinkGiftCardCustomerCommand request, CancellationToken cancellationToken)
    {
        var card = await _giftCardRepository.GetByIdAsync(request.GiftCardId)
            ?? throw new KeyNotFoundException($"Gift card with ID '{request.GiftCardId}' was not found.");

        card.CustomerId = null;
        _giftCardRepository.Update(card);
        await _uow.SaveChangesAsync(cancellationToken);

        var refreshed = await _giftCardRepository.GetByIdWithDetailsAsync(card.Id);
        return _mapper.Map<GiftCardDto>(refreshed ?? card);
    }
}
