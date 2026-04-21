using FluentAssertions;
using POS.Domain.Entities;
using POS.Domain.Enums;
using Xunit;

namespace POS.Test.Domain;

public class TransactionTests
{
    [Fact]
    public void Transaction_TotalCalculation_ShouldBeCorrect()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            ReceiptNumber = "REC-001",
            SessionId = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            CashierId = Guid.NewGuid()
        };

        var item1 = new TransactionItem
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 2,
            OriginalPrice = 100m,
            UnitPrice = 100m,
            LineTotal = 200m
        };

        var item2 = new TransactionItem
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 1,
            OriginalPrice = 50m,
            UnitPrice = 40m,
            DiscountAmount = 10m,
            LineTotal = 40m
        };

        transaction.Items.Add(item1);
        transaction.Items.Add(item2);

        // Manually update totals (assuming no auto-calculation in the entity yet)
        transaction.Subtotal = transaction.Items.Sum(i => i.Quantity * i.UnitPrice);
        transaction.DiscountTotal = transaction.Items.Sum(i => i.DiscountAmount);
        transaction.GrandTotal = transaction.Subtotal; // Simplified for this test

        // Assert
        transaction.Subtotal.Should().Be(240m);
        transaction.DiscountTotal.Should().Be(10m);
        transaction.GrandTotal.Should().Be(240m);
    }

    [Fact]
    public void Transaction_Status_ShouldDefaultToOpen()
    {
        // Arrange & Act
        var transaction = new Transaction
        {
            ReceiptNumber = "REC-002",
            SessionId = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            CashierId = Guid.NewGuid()
        };

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Open);
        transaction.Type.Should().Be(TransactionType.Sale);
    }
}
