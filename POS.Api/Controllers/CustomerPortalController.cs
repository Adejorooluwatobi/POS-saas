using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/customer-portal")]
[Authorize(Policy = "ConsumerOnly")]
public class CustomerPortalController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IGiftCardTransactionRepository _transactionRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPasswordService _passwordService;
    private readonly MediatR.IMediator _mediator;

    public CustomerPortalController(
        ICustomerRepository customerRepository,
        IGiftCardRepository giftCardRepository,
        IGiftCardTransactionRepository transactionRepository,
        ITenantContext tenantContext,
        IUnitOfWork uow,
        IMapper mapper,
        IPasswordService passwordService,
        MediatR.IMediator mediator)
    {
        _customerRepository = customerRepository;
        _giftCardRepository = giftCardRepository;
        _transactionRepository = transactionRepository;
        _tenantContext = tenantContext;
        _uow = uow;
        _mapper = mapper;
        _passwordService = passwordService;
        _mediator = mediator;
    }

    private Guid GetCurrentCustomerId()
    {
        if (_tenantContext.UserId is null)
            throw new UnauthorizedAccessException("Customer session is invalid or expired.");
        return _tenantContext.UserId.Value;
    }

    private Guid GetCurrentTenantId()
    {
        if (_tenantContext.TenantId is null)
            throw new UnauthorizedAccessException("Customer tenant context is missing.");
        return _tenantContext.TenantId.Value;
    }

    /// <summary>Gets the logged-in customer's profile, points balance, tier, and masked identity verification.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var customerId = GetCurrentCustomerId();
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer is null)
            return NotFound("Customer record not found.");

        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    /// <summary>Lists all gift and store-value cards linked to the logged-in customer.</summary>
    [HttpGet("cards")]
    public async Task<IActionResult> GetCards()
    {
        var customerId = GetCurrentCustomerId();
        var tenantId = GetCurrentTenantId();

        var paged = await _giftCardRepository.GetPagedAsync(1, 100);
        var myCards = paged.Items
            .Where(c => c.TenantId == tenantId && c.CustomerId == customerId)
            .ToList();

        return Ok(_mapper.Map<IEnumerable<GiftCardDto>>(myCards));
    }

    /// <summary>Self-service linking of a card to customer account using card number and PIN.</summary>
    [HttpPost("cards/link")]
    public async Task<IActionResult> LinkCard([FromBody] CustomerLinkCardDto dto)
    {
        var customerId = GetCurrentCustomerId();
        var tenantId = GetCurrentTenantId();

        if (string.IsNullOrWhiteSpace(dto.CardNumber))
            return BadRequest("Card number is required.");

        var card = await _giftCardRepository.GetByCardNumberAsync(tenantId, dto.CardNumber.Trim());
        if (card is null)
            return NotFound($"Card '{dto.CardNumber}' was not found.");

        if (!string.IsNullOrEmpty(card.PinHash))
        {
            if (string.IsNullOrEmpty(dto.Pin) || !_passwordService.Verify(dto.Pin, card.PinHash))
                return BadRequest("Invalid card PIN.");
        }

        if (card.CustomerId.HasValue && card.CustomerId.Value != customerId)
            return BadRequest("This card is already linked to another customer account.");

        card.CustomerId = customerId;
        _giftCardRepository.Update(card);
        await _uow.SaveChangesAsync();

        var refreshed = await _giftCardRepository.GetByIdWithDetailsAsync(card.Id);
        return Ok(_mapper.Map<GiftCardDto>(refreshed ?? card));
    }

    /// <summary>Self-service top-up using online payment gateway verification reference.</summary>
    [HttpPost("cards/topup")]
    public async Task<IActionResult> TopUpCard([FromBody] CustomerTopUpCardDto dto)
    {
        var customerId = GetCurrentCustomerId();
        var tenantId = GetCurrentTenantId();

        if (dto.Amount <= 0)
            return BadRequest("Top-up amount must be greater than zero.");

        var card = await _giftCardRepository.GetByCardNumberAsync(tenantId, dto.CardNumber.Trim());
        if (card is null)
            return NotFound($"Card '{dto.CardNumber}' was not found.");

        if (card.CustomerId != customerId)
            return Forbid("You are not authorized to top up a card not registered to your account.");

        if (!card.IsActive)
            return BadRequest("This card is currently deactivated and cannot be topped up.");

        if (card.ExpiresAt.HasValue && card.ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            return BadRequest("This card has expired and cannot receive funds.");

        var balBefore = card.Balance;
        card.Balance += dto.Amount;
        var balAfter = card.Balance;

        var tx = new GiftCardTransaction
        {
            TenantId = tenantId,
            GiftCardId = card.Id,
            Type = GiftCardTransactionType.TopUp,
            Amount = dto.Amount,
            BalanceBefore = balBefore,
            BalanceAfter = balAfter,
            Method = PaymentMethod.Card,
            Reference = dto.PaymentReference,
            Notes = $"Self-service online top-up via {dto.PaymentGateway}",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _transactionRepository.AddAsync(tx);
        _giftCardRepository.Update(card);
        await _uow.SaveChangesAsync();

        var refreshed = await _giftCardRepository.GetByIdWithDetailsAsync(card.Id);
        return Ok(_mapper.Map<GiftCardDto>(refreshed ?? card));
    }

    /// <summary>Self-service card-to-card balance transfer between customer's cards or to a friend.</summary>
    [HttpPost("cards/transfer")]
    public async Task<IActionResult> Transfer([FromBody] CustomerTransferCardBalanceDto dto)
    {
        var customerId = GetCurrentCustomerId();
        var tenantId = GetCurrentTenantId();

        if (dto.Amount <= 0)
            return BadRequest("Transfer amount must be greater than zero.");

        if (string.Equals(dto.SourceCardNumber.Trim(), dto.DestinationCardNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            return BadRequest("Source and destination cards must be different.");

        var srcCard = await _giftCardRepository.GetByCardNumberAsync(tenantId, dto.SourceCardNumber.Trim());
        if (srcCard is null)
            return NotFound($"Source card '{dto.SourceCardNumber}' was not found.");

        if (srcCard.CustomerId != customerId)
            return Forbid("You can only transfer funds from cards linked to your account.");

        var destCard = await _giftCardRepository.GetByCardNumberAsync(tenantId, dto.DestinationCardNumber.Trim());
        if (destCard is null)
            return NotFound($"Destination card '{dto.DestinationCardNumber}' was not found.");

        if (!destCard.IsActive)
            return BadRequest("Destination card is deactivated.");

        if (destCard.ExpiresAt.HasValue && destCard.ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            return BadRequest("Destination card has expired.");

        if (!string.IsNullOrEmpty(srcCard.PinHash))
        {
            if (string.IsNullOrEmpty(dto.SourcePin) || !_passwordService.Verify(dto.SourcePin, srcCard.PinHash))
                return BadRequest("Invalid source card PIN.");
        }

        if (srcCard.Balance < dto.Amount)
            return BadRequest($"Insufficient source card balance. Available: {srcCard.Balance:N2}");

        var srcBalBefore = srcCard.Balance;
        srcCard.Balance -= dto.Amount;
        var srcBalAfter = srcCard.Balance;

        var dstBalBefore = destCard.Balance;
        destCard.Balance += dto.Amount;
        var dstBalAfter = destCard.Balance;

        var now = DateTimeOffset.UtcNow;

        var srcTx = new GiftCardTransaction
        {
            TenantId = tenantId,
            GiftCardId = srcCard.Id,
            Type = GiftCardTransactionType.TransferOut,
            Amount = dto.Amount,
            BalanceBefore = srcBalBefore,
            BalanceAfter = srcBalAfter,
            Method = PaymentMethod.GiftCard,
            Reference = destCard.CardNumber,
            Notes = dto.Notes ?? $"Customer self-transfer to {destCard.CardNumber}",
            CreatedAt = now
        };

        var dstTx = new GiftCardTransaction
        {
            TenantId = tenantId,
            GiftCardId = destCard.Id,
            Type = GiftCardTransactionType.TransferIn,
            Amount = dto.Amount,
            BalanceBefore = dstBalBefore,
            BalanceAfter = dstBalAfter,
            Method = PaymentMethod.GiftCard,
            Reference = srcCard.CardNumber,
            Notes = dto.Notes ?? $"Customer self-transfer from {srcCard.CardNumber}",
            CreatedAt = now
        };

        await _transactionRepository.AddAsync(srcTx);
        await _transactionRepository.AddAsync(dstTx);
        _giftCardRepository.Update(srcCard);
        _giftCardRepository.Update(destCard);
        await _uow.SaveChangesAsync();

        var refreshed = await _giftCardRepository.GetByIdWithDetailsAsync(srcCard.Id);
        return Ok(_mapper.Map<GiftCardDto>(refreshed ?? srcCard));
    }

    /// <summary>Gets the financial transaction audit ledger for a customer's linked card.</summary>
    [HttpGet("cards/{cardId:guid}/transactions")]
    public async Task<IActionResult> GetCardTransactions(Guid cardId)
    {
        var customerId = GetCurrentCustomerId();
        var card = await _giftCardRepository.GetByIdWithDetailsAsync(cardId);
        if (card is null)
            return NotFound("Card not found.");

        if (card.CustomerId != customerId)
            return Forbid("You can only view transaction history for your own cards.");

        var txs = await _transactionRepository.GetByCardIdAsync(cardId);
        return Ok(_mapper.Map<IEnumerable<GiftCardTransactionDto>>(txs));
    }

    /// <summary>Gets the customer's sales receipts, purchase history, and spending breakdown.</summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetMyTransactions([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var customerId = GetCurrentCustomerId();
        var result = await _mediator.Send(new POS.Application.Queries.Customer.GetTransactions.GetCustomerTransactionsQuery(customerId, page, size));
        return Ok(result);
    }
}
