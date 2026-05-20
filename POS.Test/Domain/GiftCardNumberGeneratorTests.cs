using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using POS.Infrastructure.Services;
using Xunit;

namespace POS.Test.Domain;

public class GiftCardNumberGeneratorTests
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly GiftCardNumberGenerator _generator;

    public GiftCardNumberGeneratorTests()
    {
        _tenantRepository = Substitute.For<ITenantRepository>();
        _giftCardRepository = Substitute.For<IGiftCardRepository>();
        _generator = new GiftCardNumberGenerator(_tenantRepository, _giftCardRepository);
    }

    [Theory]
    [InlineData("nevermind", "NVMD")]
    [InlineData("Shoprite", "SPR")]
    [InlineData("NeverMind Store", "NVMD")]
    [InlineData("Shoprite Supermarket", "SPR")]
    public async Task GenerateCardNumberAsync_Should_UseSpecialMappings_When_KnownTenant(string businessName, string expectedPrefix)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            Slug = businessName.ToLowerInvariant().Replace(" ", "-"),
            BusinessName = businessName,
            ContactEmail = "test@example.com",
            Country = "Nigeria"
        };

        _tenantRepository.GetByIdAsync(tenantId).Returns(tenant);
        _giftCardRepository.GetByCardNumberAsync(tenantId, Arg.Any<string>()).Returns((GiftCard?)null);

        // Act
        var result = await _generator.GenerateCardNumberAsync(tenantId, CancellationToken.None);

        // Assert
        result.Should().HaveLength(16);
        result.Should().StartWith(expectedPrefix);
        result.Substring(expectedPrefix.Length).Should().MatchRegex("^\\d+$");
    }

    [Fact]
    public async Task GenerateCardNumberAsync_Should_ExtractConsonants_When_GeneralTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            Slug = "Walmart",
            BusinessName = "Walmart",
            ContactEmail = "test@example.com",
            Country = "Nigeria"
        };

        _tenantRepository.GetByIdAsync(tenantId).Returns(tenant);
        _giftCardRepository.GetByCardNumberAsync(tenantId, Arg.Any<string>()).Returns((GiftCard?)null);

        // Act
        var result = await _generator.GenerateCardNumberAsync(tenantId, CancellationToken.None);

        // Assert
        // Consonants of walmart are: W, L, M, R, T
        // First four consonants: WLMR
        result.Should().HaveLength(16);
        result.Should().StartWith("WLMR");
        result.Substring(4).Should().MatchRegex("^\\d+$");
    }

    [Fact]
    public async Task GenerateCardNumberAsync_Should_Retry_When_GeneratedNumberAlreadyExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            Slug = "nevermind",
            BusinessName = "nevermind",
            ContactEmail = "test@example.com",
            Country = "Nigeria"
        };

        _tenantRepository.GetByIdAsync(tenantId).Returns(tenant);
        
        // Mock the first call to return an existing card, and the second to return null (does not exist)
        var existingCard = new GiftCard { TenantId = tenantId, CardNumber = "NVMD123456789012", Balance = 100m, InitialValue = 100m };
        _giftCardRepository.GetByCardNumberAsync(tenantId, Arg.Any<string>())
            .Returns(existingCard, (GiftCard?)null);

        // Act
        var result = await _generator.GenerateCardNumberAsync(tenantId, CancellationToken.None);

        // Assert
        result.Should().HaveLength(16);
        result.Should().StartWith("NVMD");
        await _giftCardRepository.Received(2).GetByCardNumberAsync(tenantId, Arg.Any<string>());
    }
}
