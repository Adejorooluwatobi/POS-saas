using FluentAssertions;
using Moq;
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
    private readonly Mock<IStaffRepository> _staffRepoMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly LoginPosCommandHandler _handler;

    public LoginPosCommandHandlerTests()
    {
        _staffRepoMock = new Mock<IStaffRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _tokenServiceMock = new Mock<ITokenService>();

        _handler = new LoginPosCommandHandler(
            _staffRepoMock.Object,
            _passwordServiceMock.Object,
            _tokenServiceMock.Object
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

        _staffRepoMock.Setup(r => r.GetByEmployeeNoAsync(storeId, employeeNo)).ReturnsAsync(staff);
        _passwordServiceMock.Setup(s => s.Verify(pin, pinHash)).Returns(true);
        _tokenServiceMock.Setup(s => s.GenerateToken(staffId, employeeNo, "Cashier", tenantId, storeId)).Returns("ValidToken");

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

        _staffRepoMock.Setup(r => r.GetByEmployeeNoAsync(storeId, employeeNo)).ReturnsAsync(staff);
        _passwordServiceMock.Setup(s => s.Verify(pin, pinHash)).Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
