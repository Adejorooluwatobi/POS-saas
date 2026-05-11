using AutoMapper;
using FluentAssertions;
using NSubstitute;
using POS.Application.Commands.Inventory.Update;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Xunit;

namespace POS.Test.Application;

public class UpdateInventoryCommandHandlerTests
{
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UpdateInventoryCommandHandler _handler;

    public UpdateInventoryCommandHandlerTests()
    {
        _inventoryRepo = Substitute.For<IInventoryRepository>();
        _uow = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        
        _handler = new UpdateInventoryCommandHandler(
            _inventoryRepo,
            _uow,
            _mapper
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
            TenantId = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            VariantId = Guid.NewGuid(),
            QuantityOnHand = 10 
        };
        
        var dto = new UpdateInventoryDto { QuantityOnHand = 20 };
        var command = new UpdateInventoryCommand(inventoryId, dto);

        _inventoryRepo.GetByIdAsync(inventoryId).Returns(existingInventory);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapper.Received(1).Map(dto, existingInventory);
        _inventoryRepo.Received(1).Update(existingInventory);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowException_WhenInventoryNotFound()
    {
        // Arrange
        var inventoryId = Guid.NewGuid();
        var dto = new UpdateInventoryDto { QuantityOnHand = 20 };
        var command = new UpdateInventoryCommand(inventoryId, dto);

        _inventoryRepo.GetByIdAsync(inventoryId).Returns((Inventory?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
