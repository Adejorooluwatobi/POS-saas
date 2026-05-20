using AutoMapper;
using FluentAssertions;
using NSubstitute;
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
    private readonly ITransactionRepository _transactionRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IReceiptNumberService _receiptNumberService;
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IProductVariantRepository _variantRepo;
    private readonly ITillSessionRepository _sessionRepo;
    private readonly IGiftCardRepository _giftCardRepo;
    private readonly IPasswordService _passwordService;
    private readonly IGiftCardNumberGenerator _cardNumberGenerator;
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _transactionRepo = Substitute.For<ITransactionRepository>();
        _storeRepo = Substitute.For<IStoreRepository>();
        _uow = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _tenantContext = Substitute.For<ITenantContext>();
        _receiptNumberService = Substitute.For<IReceiptNumberService>();
        _inventoryRepo = Substitute.For<IInventoryRepository>();
        _variantRepo = Substitute.For<IProductVariantRepository>();
        _sessionRepo = Substitute.For<ITillSessionRepository>();
        _giftCardRepo = Substitute.For<IGiftCardRepository>();
        _passwordService = Substitute.For<IPasswordService>();
        _cardNumberGenerator = Substitute.For<IGiftCardNumberGenerator>();

        _handler = new CreateTransactionCommandHandler(
            _transactionRepo,
            _storeRepo,
            _uow,
            _mapper,
            _tenantContext,
            _receiptNumberService,
            _inventoryRepo,
            _variantRepo,
            _sessionRepo,
            _giftCardRepo,
            _passwordService,
            _cardNumberGenerator
        );
    }

    [Fact]
    public async Task Handle_Should_CreateTransaction_WhenRequestIsValid()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var store = new Store { Id = storeId, TenantId = Guid.NewGuid(), Code = "STORE01", Name = "Store 1", Address = "", City = "", Country = "Nigeria", Timezone = "Africa/Lagos" };
        var session = new TillSession { Id = sessionId, TerminalId = Guid.NewGuid(), StaffId = userId, Status = SessionStatus.Open };
        
        var dto = new CreateTransactionDto
        {
            StoreId = storeId,
            SessionId = sessionId,
            Items = new List<CreateTransactionItemDto>
            {
                new CreateTransactionItemDto { VariantId = Guid.NewGuid(), Quantity = 2, UnitPrice = 100, TaxRate = 0.1m }
            }
        };

        var command = new CreateTransactionCommand(dto);
        var transactionEntity = new Transaction 
        { 
            ReceiptNumber = "PENDING", 
            SessionId = sessionId, 
            StoreId = storeId, 
            CashierId = userId
        };

        _storeRepo.GetByIdAsync(storeId).Returns(store);
        _sessionRepo.GetByIdAsync(sessionId).Returns(session);
        _mapper.Map<Transaction>(dto).Returns(transactionEntity);
        _tenantContext.UserId.Returns(userId);
        _receiptNumberService.Generate(store.Code).Returns("REC-001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _transactionRepo.Received(1).AddAsync(Arg.Any<Transaction>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        
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

        _storeRepo.GetByIdAsync(storeId).Returns((Store?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
