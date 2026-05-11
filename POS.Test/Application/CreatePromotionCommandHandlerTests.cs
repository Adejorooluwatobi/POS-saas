using AutoMapper;
using FluentAssertions;
using NSubstitute;
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
    private readonly IPromotionRepository _promotionRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly CreatePromotionCommandHandler _handler;

    public CreatePromotionCommandHandlerTests()
    {
        _promotionRepo = Substitute.For<IPromotionRepository>();
        _uow = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _tenantContext = Substitute.For<ITenantContext>();

        _handler = new CreatePromotionCommandHandler(
            _promotionRepo,
            _uow,
            _mapper,
            _tenantContext
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

        _mapper.Map<Promotion>(dto).Returns(promotionEntity);
        _tenantContext.TenantId.Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        promotionEntity.TenantId.Should().Be(tenantId);
        await _promotionRepo.Received(1).AddAsync(promotionEntity);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
