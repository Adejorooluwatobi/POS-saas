using AutoMapper;
using FluentAssertions;
using Moq;
using POS.Application.Commands.Inventory.Update;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Xunit;

namespace POS.Test.Application;

public class UpdateInventoryCommandHandlerTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateInventoryCommandHandler _handler;

    public UpdateInventoryCommandHandlerTests()
    {
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new UpdateInventoryCommandHandler(
            _inventoryRepoMock.Object,
            _uowMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_UpdateInventory_WhenRequestIsValid()
    {
        // Arrange
        var inventoryId = Guid.NewGuid();
        var existingInventory = new Inventory 
        { 
            Id = inventoryId, 
            StoreId = Guid.NewGuid(),
            VariantId = Guid.NewGuid(),
            QuantityOnHand = 10 
        };
        
        var dto = new UpdateInventoryDto { QuantityOnHand = 20 };
        var command = new UpdateInventoryCommand(inventoryId, dto);

        _inventoryRepoMock.Setup(r => r.GetByIdAsync(inventoryId)).ReturnsAsync(existingInventory);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapperMock.Verify(m => m.Map(dto, existingInventory), Times.Once);
        _inventoryRepoMock.Verify(r => r.Update(existingInventory), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowException_WhenInventoryNotFound()
    {
        // Arrange
        var inventoryId = Guid.NewGuid();
        var dto = new UpdateInventoryDto { QuantityOnHand = 20 };
        var command = new UpdateInventoryCommand(inventoryId, dto);

        _inventoryRepoMock.Setup(r => r.GetByIdAsync(inventoryId)).ReturnsAsync((Inventory?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
