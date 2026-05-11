using AutoMapper;
using FluentAssertions;
using NSubstitute;
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
    private readonly IProductRepository _productRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IProductVariantRepository _variantRepo;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepo = Substitute.For<IProductRepository>();
        _uow = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _tenantContext = Substitute.For<ITenantContext>();
        _variantRepo = Substitute.For<IProductVariantRepository>();

        _handler = new CreateProductCommandHandler(
            _productRepo,
            _variantRepo,
            _uow,
            _mapper,
            _tenantContext
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

        _tenantContext.TenantId.Returns(tenantId);
        _mapper.Map<Product>(dto).Returns(productEntity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _productRepo.Received(1).AddAsync(productEntity);
        await _variantRepo.Received(1).AddAsync(Arg.Any<ProductVariant>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
