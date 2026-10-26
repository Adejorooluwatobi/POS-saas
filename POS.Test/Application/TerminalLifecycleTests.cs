using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using POS.Application.Commands.Auth.LoginAdmin;
using POS.Application.Commands.Terminal.Create;
using POS.Application.Commands.Terminal.Disconnect;
using POS.Application.Commands.Terminal.Pair;
using POS.Application.Commands.Terminal.ReconnectPin;
using POS.Application.DTOs;
using POS.Application.DTOs.Auth;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Xunit;

namespace POS.Test.Application;

public class TerminalLifecycleTests
{
    private readonly ITerminalRepository _terminalRepo;
    private readonly ITenantSubscriptionRepository _subRepo;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public TerminalLifecycleTests()
    {
        _terminalRepo = Substitute.For<ITerminalRepository>();
        _subRepo = Substitute.For<ITenantSubscriptionRepository>();
        _tenantContext = Substitute.For<ITenantContext>();
        _uow = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
    }

    private Store CreateStore(Guid tenantId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Code = "STR-001",
        Name = "Main Branch",
        Address = "123 Commercial Ave",
        City = "Lagos",
        Country = "Nigeria",
        Timezone = "Africa/Lagos",
        IsActive = true
    };

    [Fact]
    public async Task DisconnectTerminal_Should_MarkStatusDisconnected_And_ClearDeviceToken()
    {
        // Arrange
        var terminalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var store = CreateStore(tenantId);
        var terminal = new Terminal
        {
            Id = terminalId,
            StoreId = store.Id,
            Store = store,
            TerminalCode = "TRM-TEST01",
            Status = TerminalStatus.Online,
            DeviceToken = "active-device-token",
            LeaseExpiresAt = DateTimeOffset.UtcNow.AddDays(20)
        };

        _terminalRepo.GetByIdAsync(terminalId).Returns(terminal);
        _tenantContext.TenantId.Returns(tenantId);
        _tenantContext.SystemRole.Returns("TenantAdmin");
        _tenantContext.UserName.Returns("Boss Admin");

        var handler = new DisconnectTerminalCommandHandler(_terminalRepo, _tenantContext, _uow, _mapper);

        // Act
        var result = await handler.Handle(new DisconnectTerminalCommand(terminalId), CancellationToken.None);

        // Assert
        terminal.Status.Should().Be(TerminalStatus.Disconnected);
        terminal.DeviceToken.Should().BeNull();
        terminal.LeaseExpiresAt.Should().BeNull();
        terminal.DisconnectedAt.Should().NotBeNull();
        terminal.DisconnectedBy.Should().Be("Boss Admin");
        _terminalRepo.Received(1).Update(terminal);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateReconnectPin_Should_Create6DigitPin_With48HourExpiry()
    {
        // Arrange
        var terminalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var store = CreateStore(tenantId);
        var terminal = new Terminal
        {
            Id = terminalId,
            StoreId = store.Id,
            Store = store,
            TerminalCode = "TRM-TEST01",
            Status = TerminalStatus.Disconnected
        };

        _terminalRepo.GetByIdAsync(terminalId).Returns(terminal);
        _terminalRepo.GetQueryable().Returns(new List<Terminal> { terminal }.AsQueryable());
        _tenantContext.TenantId.Returns(tenantId);
        _tenantContext.SystemRole.Returns("TenantAdmin");

        var sub = new TenantSubscription
        {
            TenantId = tenantId,
            MaxTerminals = 5,
            CurrentPeriodEnd = DateTimeOffset.UtcNow.AddDays(30),
            Status = SubscriptionStatus.Active
        };
        _subRepo.GetByTenantAsync(tenantId).Returns(sub);

        var handler = new GenerateTerminalReconnectPinCommandHandler(_terminalRepo, _subRepo, _tenantContext, _uow, _mapper);

        // Act
        var result = await handler.Handle(new GenerateTerminalReconnectPinCommand(terminalId), CancellationToken.None);

        // Assert
        result.Pin.Should().HaveLength(6);
        result.Pin.All(char.IsDigit).Should().BeTrue();
        terminal.PairingCode.Should().Be(result.Pin);
        terminal.PairingCodeExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow.AddHours(47));
        _terminalRepo.Received(1).Update(terminal);
    }

    [Fact]
    public async Task PairTerminal_Should_Grant30DayLease_WhenValidPinProvided()
    {
        // Arrange
        var pin = "123456";
        var tenantId = Guid.NewGuid();
        var store = CreateStore(tenantId);
        var terminal = new Terminal
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            Store = store,
            TerminalCode = "TRM-TEST01",
            Status = TerminalStatus.Disconnected,
            PairingCode = pin,
            PairingCodeExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };

        _terminalRepo.GetByPairingCodeAsync(pin).Returns(terminal);
        _terminalRepo.GetQueryable().Returns(new List<Terminal> { terminal }.AsQueryable());

        var sub = new TenantSubscription
        {
            TenantId = tenantId,
            MaxTerminals = 2,
            CurrentPeriodEnd = DateTimeOffset.UtcNow.AddDays(30),
            Status = SubscriptionStatus.Active
        };
        _subRepo.GetByTenantAsync(tenantId).Returns(sub);

        var handler = new PairTerminalCommandHandler(_terminalRepo, _subRepo, _uow, _mapper);

        // Act
        var result = await handler.Handle(new PairTerminalCommand(pin), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        terminal.Status.Should().Be(TerminalStatus.Online);
        terminal.DeviceToken.Should().NotBeNullOrEmpty();
        terminal.LeaseExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow.AddDays(29));
        terminal.PairingCode.Should().BeNull();
    }

    [Fact]
    public async Task LoginAdmin_Should_ThrowException_WhenSubscriptionExpired()
    {
        // Arrange
        var staffRepo = Substitute.For<IStaffRepository>();
        var storeRepo = Substitute.For<IStoreRepository>();
        var tenantRepo = Substitute.For<ITenantRepository>();
        var subRepo = Substitute.For<ITenantSubscriptionRepository>();
        var passwordService = Substitute.For<IPasswordService>();
        var tokenService = Substitute.For<ITokenService>();

        var tenantId = Guid.NewGuid();
        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = "admin@store.com",
            PasswordHash = "hashed_pw",
            PinHash = "hashed_pin",
            FirstName = "Super",
            LastName = "Manager",
            EmployeeNo = "EMP-001",
            HiredAt = DateOnly.FromDateTime(DateTime.Today),
            SystemRole = SystemRole.TenantAdmin,
            IsActive = true
        };

        staffRepo.GetByEmailAsync("admin@store.com").Returns(staff);
        passwordService.Verify("secret", "hashed_pw").Returns(true);
        tenantRepo.GetByIdAsync(tenantId).Returns(new Tenant
        {
            Slug = "test",
            BusinessName = "Test Store",
            ContactEmail = "a@a.com",
            Country = "Nigeria",
            IsActive = true
        });

        // Expired subscription
        subRepo.GetByTenantAsync(tenantId).Returns(new TenantSubscription
        {
            TenantId = tenantId,
            CurrentPeriodEnd = DateTimeOffset.UtcNow.AddDays(-1),
            Status = SubscriptionStatus.Trial
        });

        var handler = new LoginAdminCommandHandler(staffRepo, storeRepo, tenantRepo, subRepo, passwordService, tokenService);

        // Act
        Func<Task> act = async () => await handler.Handle(new LoginAdminCommand(new AdminLoginDto("admin@store.com", "secret")), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*expired*");
    }
}
