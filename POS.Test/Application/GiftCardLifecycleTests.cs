using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using POS.Application.Commands.GiftCard.ReplaceLost;
using POS.Application.Commands.GiftCard.SetStatus;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using POS.Infrastructure.Services;
using Xunit;

namespace POS.Test.Application;

public class GiftCardLifecycleTests
{
    private readonly IGiftCardRepository _cardRepo = Substitute.For<IGiftCardRepository>();
    private readonly IGiftCardTransactionRepository _txRepo = Substitute.For<IGiftCardTransactionRepository>();
    private readonly IGiftCardNumberGenerator _generator = Substitute.For<IGiftCardNumberGenerator>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly Guid _tenantId = Guid.NewGuid();

    public GiftCardLifecycleTests()
    {
        _tenantContext.TenantId.Returns(_tenantId);
        _tenantContext.UserId.Returns(Guid.NewGuid());
        _passwordService.Hash(Arg.Any<string>()).Returns("hashed_pin");
        _passwordService.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
    }

    [Fact]
    public async Task SetStatus_WhenCardIsExpired_CannotBeReactivated()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var expiredCard = new GiftCard
        {
            Id = cardId,
            TenantId = _tenantId,
            CardNumber = "NVMD123456789012",
            Balance = 2000m,
            InitialValue = 2000m,
            IsActive = false,
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)) // Expired!
        };

        _cardRepo.GetByIdWithDetailsAsync(cardId).Returns(expiredCard);

        var handler = new SetGiftCardStatusCommandHandler(_cardRepo, _uow, _mapper, _tenantContext);
        var command = new SetGiftCardStatusCommand(cardId, new SetGiftCardStatusDto { IsActive = true, Reason = "Reactivate" });

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Expired cards can NEVER be reactivated*");
    }

    [Fact]
    public async Task SetStatus_WhenCardIsNotExpired_CanBeActivated()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var validInactiveCard = new GiftCard
        {
            Id = cardId,
            TenantId = _tenantId,
            CardNumber = "NVMD123456789012",
            Balance = 2000m,
            InitialValue = 2000m,
            IsActive = false,
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6))
        };

        _cardRepo.GetByIdWithDetailsAsync(cardId).Returns(validInactiveCard);
        _mapper.Map<GiftCardDto>(Arg.Any<GiftCard>()).Returns(new GiftCardDto
        {
            Id = cardId,
            CardNumber = "NVMD123456789012",
            Balance = 2000m,
            IsActive = true
        });

        var handler = new SetGiftCardStatusCommandHandler(_cardRepo, _uow, _mapper, _tenantContext);
        var command = new SetGiftCardStatusCommand(cardId, new SetGiftCardStatusDto { IsActive = true, Reason = "Cashier activation" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        validInactiveCard.IsActive.Should().BeTrue();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceLostCard_WhenCardIsExpiredAndHasNoCustomer_ThrowsException()
    {
        // Arrange
        var lostCardNumber = "NVMD999988887777";
        var expiredUnlinkedCard = new GiftCard
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            CardNumber = lostCardNumber,
            Balance = 5000m,
            InitialValue = 5000m,
            IsActive = false,
            CustomerId = null, // No customer!
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5))
        };

        _cardRepo.GetByCardNumberAsync(_tenantId, lostCardNumber).Returns(expiredUnlinkedCard);

        var handler = new ReplaceLostCardCommandHandler(
            _cardRepo, _txRepo, _generator, _passwordService, _uow, _mapper, _tenantContext);

        var command = new ReplaceLostCardCommand(new ReplaceLostGiftCardDto
        {
            LostCardNumber = lostCardNumber,
            BypassVerification = true
        });

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Expired card must be linked to a registered customer before balance can be transferred*");
    }

    [Fact]
    public async Task ReplaceLostCard_SuccessfullyMigratesBalanceAndFreezesLostCard()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var lostCardNumber = "NVMD000011112222";
        var lostCard = new GiftCard
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            CardNumber = lostCardNumber,
            Balance = 7500m,
            InitialValue = 7500m,
            IsActive = true,
            CustomerId = customerId,
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        };

        _cardRepo.GetByCardNumberAsync(_tenantId, lostCardNumber).Returns(lostCard);
        _generator.GenerateCardNumberAsync(_tenantId, Arg.Any<CancellationToken>()).Returns("NVMD888899990000");

        _mapper.Map<GiftCardDto>(Arg.Any<GiftCard>()).Returns(ci => new GiftCardDto
        {
            Id = Guid.NewGuid(),
            CardNumber = "NVMD888899990000",
            Balance = 7500m,
            CustomerId = customerId,
            IsActive = true
        });

        var handler = new ReplaceLostCardCommandHandler(
            _cardRepo, _txRepo, _generator, _passwordService, _uow, _mapper, _tenantContext);

        var command = new ReplaceLostCardCommand(new ReplaceLostGiftCardDto
        {
            LostCardNumber = lostCardNumber,
            ActivateNewCard = true,
            Reason = "Customer misplaced wallet",
            BypassVerification = true
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        lostCard.IsActive.Should().BeFalse();
        lostCard.Balance.Should().Be(0m);
        await _cardRepo.Received(1).AddAsync(Arg.Is<GiftCard>(c => 
            c.Balance == 7500m && 
            c.CustomerId == customerId && 
            c.IsActive == true));

        await _txRepo.Received(2).AddAsync(Arg.Any<GiftCardTransaction>()); // Debit + Credit
        await _uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void AesEncryptionService_EncryptsDecryptsAndMasksCorrectly()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        config["Encryption:Key"].Returns((string?)null); // Use default fallback key
        var service = new AesEncryptionService(config);

        var nin = "12345678901";

        // Act
        var encrypted = service.Encrypt(nin);
        var decrypted = service.Decrypt(encrypted);
        var masked = service.Mask(decrypted, 4);

        // Assert
        encrypted.Should().NotBe(nin);
        decrypted.Should().Be(nin);
        masked.Should().Be("*******8901");
    }
}
