using AutoMapper;
using FluentAssertions;
using Moq;
using POS.Application.Commands.Transaction.Create;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using POS.Domain.Enums;
using Xunit;

namespace POS.Test.Application;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly Mock<IStoreRepository> _storeRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IReceiptNumberService> _receiptNumberServiceMock;
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _storeRepoMock = new Mock<IStoreRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _tenantContextMock = new Mock<ITenantContext>();
        _receiptNumberServiceMock = new Mock<IReceiptNumberService>();

        _handler = new CreateTransactionCommandHandler(
            _transactionRepoMock.Object,
            _storeRepoMock.Object,
            _uowMock.Object,
            _mapperMock.Object,
            _tenantContextMock.Object,
            _receiptNumberServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreateTransaction_WhenRequestIsValid()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var store = new Store { Id = storeId, TenantId = Guid.NewGuid(), Code = "STORE01", Name = "Store 1", Address = "", City = "", Country = "Nigeria", Timezone = "Africa/Lagos" };
        
        var dto = new CreateTransactionDto
        {
            StoreId = storeId,
            Items = new List<CreateTransactionItemDto>
            {
                new CreateTransactionItemDto { VariantId = Guid.NewGuid(), Quantity = 2, UnitPrice = 100, TaxRate = 0.1m }
            }
        };

        var command = new CreateTransactionCommand(dto);
        var transactionEntity = new Transaction { ReceiptNumber = "PENDING", SessionId = Guid.NewGuid(), StoreId = storeId, CashierId = userId };

        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId)).ReturnsAsync(store);
        _mapperMock.Setup(m => m.Map<Transaction>(dto)).Returns(transactionEntity);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);
        _receiptNumberServiceMock.Setup(s => s.Generate(store.Code)).Returns("REC-001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _transactionRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        transactionEntity.GrandTotal.Should().Be(220m); // (100 * 2) + (100 * 2 * 0.1)
        transactionEntity.ReceiptNumber.Should().Be("REC-001");
    }

    [Fact]
    public async Task Handle_Should_ThrowException_WhenStoreNotFound()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var dto = new CreateTransactionDto { StoreId = storeId };
        var command = new CreateTransactionCommand(dto);

        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId)).ReturnsAsync((Store?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
