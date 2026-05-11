using AutoMapper;
using FluentAssertions;
using NSubstitute;
using POS.Application.Commands.Category.Create;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using POS.Domain.Enums;
using Xunit;

namespace POS.Test.Application;

public class CreateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepo = Substitute.For<ICategoryRepository>();
        _uow = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _tenantContext = Substitute.For<ITenantContext>();

        _handler = new CreateCategoryCommandHandler(
            _categoryRepo,
            _uow,
            _mapper,
            _tenantContext
        );
    }

    [Fact]
    public async Task Handle_Should_CreateCategory_WhenRequestIsValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dto = new CreateCategoryDto 
        { 
            Name = "Laptops", 
            Slug = "laptops"
        };
        var command = new CreateCategoryCommand(dto);
        var categoryEntity = new Category 
        { 
            Name = dto.Name,
            Slug = dto.Slug,
            TenantId = tenantId
        };

        _tenantContext.TenantId.Returns(tenantId);
        _mapper.Map<Category>(dto).Returns(categoryEntity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _categoryRepo.Received(1).AddAsync(categoryEntity);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
