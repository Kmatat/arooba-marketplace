using Arooba.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Arooba.Domain.Tests.ValueObjects;

public class MoneyTests
{
    #region Creation

    [Fact]
    public void Constructor_WithValidAmount_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = new Money(100m);

        // Assert
        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("EGP");
    }

    [Fact]
    public void Constructor_WithSpecifiedCurrency_ShouldUseThatCurrency()
    {
        // Arrange & Act
        var money = new Money(50m, "USD");

        // Assert
        money.Amount.Should().Be(50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithZeroAmount_ShouldSucceed()
    {
        // Arrange & Act
        var money = new Money(0m);

        // Assert
        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldSucceed()
    {
        // Arrange & Act
        var money = new Money(-50m);

        // Assert
        money.Amount.Should().Be(-50m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCurrency_ShouldThrowArgumentException(string? currency)
    {
        // Arrange & Act
        var act = () => new Money(100m, currency!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("currency");
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmountInDefaultCurrency()
    {
        // Arrange & Act
        var money = Money.Zero();

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("EGP");
    }

    [Fact]
    public void Zero_WithSpecifiedCurrency_ShouldReturnZeroInThatCurrency()
    {
        // Arrange & Act
        var money = Money.Zero("USD");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("USD");
    }

    #endregion

    #region Addition

    [Fact]
    public void Addition_WithSameCurrency_ShouldAddAmounts()
    {
        // Arrange
        var money1 = new Money(100m);
        var money2 = new Money(50m);

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("EGP");
    }

    [Fact]
    public void Addition_WithDifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var egp = new Money(100m, "EGP");
        var usd = new Money(50m, "USD");

        // Act
        var act = () => egp + usd;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Theory]
    [InlineData(100, 200, 300)]
    [InlineData(0, 50, 50)]
    [InlineData(99.99, 0.01, 100)]
    [InlineData(-50, 100, 50)]
    public void Addition_VariousAmounts_ShouldReturnCorrectSum(
        decimal amount1, decimal amount2, decimal expected)
    {
        // Arrange
        var money1 = new Money(amount1);
        var money2 = new Money(amount2);

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(expected);
    }

    #endregion

    #region Subtraction

    [Fact]
    public void Subtraction_WithSameCurrency_ShouldSubtractAmounts()
    {
        // Arrange
        var money1 = new Money(100m);
        var money2 = new Money(30m);

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(70m);
        result.Currency.Should().Be("EGP");
    }

    [Fact]
    public void Subtraction_WithDifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var egp = new Money(100m, "EGP");
        var usd = new Money(50m, "USD");

        // Act
        var act = () => egp - usd;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtraction_ResultingInNegative_ShouldSucceed()
    {
        // Arrange
        var money1 = new Money(30m);
        var money2 = new Money(100m);

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(-70m);
    }

    #endregion

    #region Multiplication

    [Theory]
    [InlineData(100, 2, 200)]
    [InlineData(100, 0.5, 50)]
    [InlineData(100, 0, 0)]
    [InlineData(100, -1, -100)]
    [InlineData(33.33, 3, 99.99)]
    public void Multiplication_MoneyTimesFactor_ShouldReturnCorrectProduct(
        decimal amount, decimal factor, decimal expected)
    {
        // Arrange
        var money = new Money(amount);

        // Act
        var result = money * factor;

        // Assert
        result.Amount.Should().Be(expected);
        result.Currency.Should().Be("EGP");
    }

    [Fact]
    public void Multiplication_FactorTimesMoney_ShouldReturnCorrectProduct()
    {
        // Arrange
        var money = new Money(100m);

        // Act
        var result = 2.5m * money;

        // Assert
        result.Amount.Should().Be(250m);
        result.Currency.Should().Be("EGP");
    }

    [Fact]
    public void Multiplication_ShouldPreserveCurrency()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money * 3m;

        // Assert
        result.Currency.Should().Be("USD");
    }

    #endregion

    #region Equality

    [Fact]
    public void Equality_SameAmountAndCurrency_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100m, "EGP");
        var money2 = new Money(100m, "EGP");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentAmounts_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100m);
        var money2 = new Money(200m);

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentCurrencies_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100m, "EGP");
        var money2 = new Money(100m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var money = new Money(1234.56m, "EGP");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Contain("1,234.56");
        result.Should().Contain("EGP");
    }

    [Fact]
    public void ToString_ZeroAmount_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.Zero();

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Contain("0.00");
        result.Should().Contain("EGP");
    }

    #endregion

    #region Currency Case Sensitivity

    [Fact]
    public void Addition_WithSameCurrencyDifferentCase_ShouldSucceed()
    {
        // Arrange
        var money1 = new Money(100m, "EGP");
        var money2 = new Money(50m, "egp");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150m);
    }

    #endregion
}
