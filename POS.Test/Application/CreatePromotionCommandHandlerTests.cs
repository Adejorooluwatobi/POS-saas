using AutoMapper;
using FluentAssertions;
using Moq;
using POS.Application.Commands.Promotion.Create;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using POS.Domain.Enums;
using Xunit;

namespace POS.Test.Application;

public class CreatePromotionCommandHandlerTests
{
    private readonly Mock<IPromotionRepository> _promotionRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly CreatePromotionCommandHandler _handler;

    public CreatePromotionCommandHandlerTests()
    {
        _promotionRepoMock = new Mock<IPromotionRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _tenantContextMock = new Mock<ITenantContext>();

        _handler = new CreatePromotionCommandHandler(
            _promotionRepoMock.Object,
            _uowMock.Object,
            _mapperMock.Object,
            _tenantContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreatePromotion_WhenRequestIsValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dto = new CreatePromotionDto 
        { 
            Name = "Holiday Sale", 
            Type = PromotionType.Percent,
            Scope = PromotionScope.Cart,
            Value = 10,
            StartsAt = DateTimeOffset.UtcNow
        };
        var command = new CreatePromotionCommand(dto);
        var promotionEntity = new Promotion 
        { 
            Name = dto.Name,
            TenantId = tenantId,
            Type = dto.Type,
            Scope = dto.Scope,
            Value = dto.Value,
            StartsAt = dto.StartsAt
        };

        _mapperMock.Setup(m => m.Map<Promotion>(dto)).Returns(promotionEntity);
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        promotionEntity.TenantId.Should().Be(tenantId);
        _promotionRepoMock.Verify(r => r.AddAsync(promotionEntity), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
