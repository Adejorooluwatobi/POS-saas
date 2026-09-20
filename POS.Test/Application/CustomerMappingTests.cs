using System;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POS.Application;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Infrastructure.Services;
using Xunit;

namespace POS.Test.Application;

public class CustomerMappingTests
{
    private readonly IMapper _mapper;
    private readonly IEncryptionService _encryptionService;

    public CustomerMappingTests()
    {
        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
        {
            {"Encryption:Key", "Unit_Test_Secret_Key_At_Least_32_Characters_Long!"}
        };
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _encryptionService = new AesEncryptionService(config);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();
        services.AddSingleton<IEncryptionService>(_encryptionService);
        var sp = services.BuildServiceProvider();

        _mapper = sp.GetRequiredService<IMapper>();
    }

    [Fact]
    public void CustomerToCustomerDto_WithEncryptedIdentityNumber_MapsSuccessfullyWithoutThrowing()
    {
        // Arrange
        var plainNIN = "12345678901";
        var encryptedNIN = _encryptionService.Encrypt(plainNIN);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            FirstName = "Ade",
            LastName = "Ogun",
            Email = "ade@test.com",
            Phone = "08012345678",
            LoyaltyCardNo = "TEST-LOY-123456",
            IdentityType = IdentityType.NIN,
            EncryptedIdentityNumber = encryptedNIN,
            IsIdentityVerified = true
        };

        // Act
        var dto = _mapper.Map<CustomerDto>(customer);

        // Assert
        dto.Should().NotBeNull();
        dto.FirstName.Should().Be("Ade");
        dto.LastName.Should().Be("Ogun");
        dto.MaskedIdentityNumber.Should().NotBeNullOrEmpty();
        dto.MaskedIdentityNumber.Should().EndWith("8901");
    }

    [Fact]
    public void CustomerToCustomerDto_WithoutIdentityNumber_MapsSuccessfully()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            FirstName = "Hardey",
            LastName = "Oluwa",
            Email = "hardey@test.com",
            Phone = "07035159871",
            LoyaltyCardNo = "CHI ST 2-LOY-477687",
            IdentityType = IdentityType.None,
            EncryptedIdentityNumber = null,
            IsIdentityVerified = false
        };

        // Act
        var dto = _mapper.Map<CustomerDto>(customer);

        // Assert
        dto.Should().NotBeNull();
        dto.FirstName.Should().Be("Hardey");
        dto.MaskedIdentityNumber.Should().BeNull();
    }

    [Fact]
    public void CustomerToCustomerDto_WithRegisteredStoreAndStaff_MapsNamesProperly()
    {
        // Arrange
        var store = new Store
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Lekki Superstore",
            Code = "LEK-01",
            Address = "12 Admiralty Way",
            City = "Lekki",
            Country = "Nigeria",
            Timezone = "Africa/Lagos"
        };

        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            TenantId = store.TenantId,
            FirstName = "Amina",
            LastName = "Bello",
            EmployeeNo = "EMP-001",
            Email = "amina@test.com",
            PinHash = "hash",
            HiredAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = store.TenantId,
            FirstName = "Chidi",
            LastName = "Okeke",
            RegisteredStore = store,
            RegisteredStoreId = store.Id,
            RegisteredByStaff = staff,
            RegisteredByStaffId = staff.Id,
            IsSelfRegistered = false
        };

        // Act
        var dto = _mapper.Map<CustomerDto>(customer);

        // Assert
        dto.RegisteredStoreName.Should().Be("Lekki Superstore");
        dto.RegisteredByStaffName.Should().Be("Amina Bello");
        dto.IsSelfRegistered.Should().BeFalse();
    }

    [Fact]
    public void CustomerToCustomerDto_WhenSelfRegistered_MapsOnlineBadge()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            FirstName = "Zainab",
            LastName = "Aliyu",
            RegisteredStore = null,
            IsSelfRegistered = true
        };

        // Act
        var dto = _mapper.Map<CustomerDto>(customer);

        // Assert
        dto.IsSelfRegistered.Should().BeTrue();
        dto.RegisteredStoreName.Should().Be("Online (Self-Registered)");
    }

    [Fact]
    public void CustomerToCustomerDto_WithCompletedTransactions_ComputesTotalSpendAndVisits()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            TenantId = Guid.NewGuid(),
            FirstName = "Emeka",
            LastName = "Nnamdi",
            Transactions = new List<Transaction>
            {
                new() { ReceiptNumber = "REC-01", GrandTotal = 15000m, Status = TransactionStatus.Completed, SessionId = Guid.NewGuid(), StoreId = Guid.NewGuid(), CashierId = Guid.NewGuid() },
                new() { ReceiptNumber = "REC-02", GrandTotal = 25000m, Status = TransactionStatus.Completed, SessionId = Guid.NewGuid(), StoreId = Guid.NewGuid(), CashierId = Guid.NewGuid() },
                new() { ReceiptNumber = "REC-03", GrandTotal = 5000m, Status = TransactionStatus.Voided, SessionId = Guid.NewGuid(), StoreId = Guid.NewGuid(), CashierId = Guid.NewGuid() }
            }
        };

        // Act
        var dto = _mapper.Map<CustomerDto>(customer);

        // Assert
        dto.TotalSpend.Should().Be(40000m);
        dto.TotalVisits.Should().Be(2);
    }
}
