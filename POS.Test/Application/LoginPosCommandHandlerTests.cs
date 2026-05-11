using FluentAssertions;
using NSubstitute;
using POS.Application.Commands.Auth.LoginPos;
using POS.Application.DTOs.Auth;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Xunit;

namespace POS.Test.Application;

public class LoginPosCommandHandlerTests
{
    private readonly IStaffRepository _staffRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly ITenantRepository _tenantRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly LoginPosCommandHandler _handler;

    public LoginPosCommandHandlerTests()
    {
        _staffRepo = Substitute.For<IStaffRepository>();
        _storeRepo = Substitute.For<IStoreRepository>();
        _tenantRepo = Substitute.For<ITenantRepository>();
        _passwordService = Substitute.For<IPasswordService>();
        _tokenService = Substitute.For<ITokenService>();

        _handler = new LoginPosCommandHandler(
            _staffRepo,
            _storeRepo,
            _tenantRepo,
            _passwordService,
            _tokenService
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnAuthResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var employeeNo = "EMP001";
        var pin = "1234";
        var pinHash = "HashedPin";

        var dto = new PosLoginDto(storeId, employeeNo, pin);
        var command = new LoginPosCommand(dto);

        var staff = new Staff 
        { 
            Id = staffId,
            TenantId = tenantId,
            StoreId = storeId,
            EmployeeNo = employeeNo,
            PinHash = pinHash,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            HiredAt = DateOnly.FromDateTime(DateTime.Now),
            SystemRole = SystemRole.Cashier
        };

        _staffRepo.GetByEmployeeNoAsync(storeId, employeeNo).Returns(staff);
        _tenantRepo.GetByIdAsync(tenantId).Returns(new Tenant 
        { 
            Id = tenantId, 
            IsActive = true, 
            BusinessName = "Test Tenant", 
            Slug = "test",
            ContactEmail = "admin@test.com",
            Country = "Nigeria"
        });
        _storeRepo.GetByIdAsync(storeId).Returns(new Store 
        { 
            Id = storeId, 
            IsActive = true, 
            Name = "Test Store", 
            TenantId = tenantId,
            Code = "ST001",
            Address = "123 Street",
            City = "Lagos",
            Country = "Nigeria",
            Timezone = "Africa/Lagos"
        });
        _passwordService.Verify(pin, pinHash).Returns(true);
        _tokenService.GenerateToken(staffId, employeeNo, "Cashier", "John Doe", tenantId, storeId).Returns("ValidToken");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("ValidToken");
        result.Role.Should().Be("Cashier");
        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_Should_ThrowException_WhenPinIsInvalid()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var employeeNo = "EMP001";
        var pin = "wrong";
        var pinHash = "HashedPin";

        var dto = new PosLoginDto(storeId, employeeNo, pin);
        var command = new LoginPosCommand(dto);

        var staff = new Staff 
        { 
            EmployeeNo = employeeNo, 
            PinHash = pinHash,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            HiredAt = DateOnly.FromDateTime(DateTime.Now),
            TenantId = Guid.NewGuid()
        };

        _staffRepo.GetByEmployeeNoAsync(storeId, employeeNo).Returns(staff);
        _tenantRepo.GetByIdAsync(Arg.Any<Guid>()).Returns(new Tenant 
        { 
            IsActive = true,
            Slug = "test",
            BusinessName = "Test",
            ContactEmail = "test@test.com",
            Country = "Nigeria"
        });
        _storeRepo.GetByIdAsync(Arg.Any<Guid>()).Returns(new Store 
        { 
            IsActive = true,
            TenantId = Guid.NewGuid(),
            Code = "ST001",
            Name = "Test Store",
            Address = "123 Street",
            City = "Lagos",
            Country = "Nigeria",
            Timezone = "Africa/Lagos"
        });
        _passwordService.Verify(pin, pinHash).Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
