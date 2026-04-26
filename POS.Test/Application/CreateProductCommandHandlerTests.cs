using AutoMapper;
using FluentAssertions;
using Moq;
using POS.Application.Commands.Product.Create;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using POS.Domain.Enums;
using Xunit;

namespace POS.Test.Application;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IProductVariantRepository> _variantRepoMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _tenantContextMock = new Mock<ITenantContext>();
        _variantRepoMock = new Mock<IProductVariantRepository>();

        _handler = new CreateProductCommandHandler(
            _productRepoMock.Object,
            _variantRepoMock.Object,
            _uowMock.Object,
            _mapperMock.Object,
            _tenantContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreateProduct_WhenRequestIsValid()
    {
        // Arrange
        var dto = new CreateProductDto 
        { 
            Name = "New Product", 
            MasterSku = "SKU-001",
            TaxCategory = TaxCategory.Standard
        };
        var command = new CreateProductCommand(dto);
        var tenantId = Guid.NewGuid();
        var productEntity = new Product { Name = dto.Name, MasterSku = dto.MasterSku, TenantId = tenantId };

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _mapperMock.Setup(m => m.Map<Product>(dto)).Returns(productEntity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productRepoMock.Verify(r => r.AddAsync(productEntity), Times.Once);
        _variantRepoMock.Verify(r => r.AddAsync(It.IsAny<ProductVariant>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
