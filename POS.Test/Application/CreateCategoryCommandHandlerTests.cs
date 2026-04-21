using AutoMapper;
using FluentAssertions;
using Moq;
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
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _handler = new CreateCategoryCommandHandler(
            _categoryRepoMock.Object,
            _uowMock.Object,
            _mapperMock.Object
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

        _mapperMock.Setup(m => m.Map<Category>(dto)).Returns(categoryEntity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepoMock.Verify(r => r.AddAsync(categoryEntity), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
