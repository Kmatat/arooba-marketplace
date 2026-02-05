using Arooba.Domain.ValueObjects;
using FluentAssertions;

namespace Arooba.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    #region Valid Egyptian Phone Numbers

    [Theory]
    [InlineData("+201012345678")]
    [InlineData("+201112345678")]
    [InlineData("+201212345678")]
    [InlineData("+201512345678")]
    [InlineData("+201000000000")]
    [InlineData("+201999999999")]
    public void Constructor_WithValidEgyptianNumber_ShouldCreatePhoneNumber(string validNumber)
    {
        // Arrange & Act
        var phone = new PhoneNumber(validNumber);

        // Assert
        phone.Value.Should().Be(validNumber);
    }

    [Fact]
    public void Constructor_WithValidNumber_ShouldStoreValue()
    {
        // Arrange
        const string number = "+201012345678";

        // Act
        var phone = new PhoneNumber(number);

        // Assert
        phone.Value.Should().Be(number);
    }

    #endregion

    #region Invalid Phone Numbers

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmpty_ShouldThrowArgumentException(string? value)
    {
        // Arrange & Act
        var act = () => new PhoneNumber(value!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData("01012345678")]         // Missing country code
    [InlineData("+20012345678")]        // Invalid - starts with 0 after +20
    [InlineData("+201")]                // Too short
    [InlineData("+2010123456789")]      // Too long (13 digits after +20)
    [InlineData("+20101234567")]        // Too short (9 digits after +201)
    [InlineData("+20201234567")]        // Invalid - starts with 2 after +20
    [InlineData("+20301234567")]        // Invalid - starts with 3 after +20
    [InlineData("201012345678")]        // Missing + prefix
    [InlineData("+971501234567")]       // UAE number, not Egyptian
    [InlineData("+11234567890")]        // US number, not Egyptian
    [InlineData("abc")]                 // Non-numeric
    [InlineData("+20abcdefghij")]       // Non-numeric after country code
    public void Constructor_WithInvalidFormat_ShouldThrowArgumentException(string invalidNumber)
    {
        // Arrange & Act
        var act = () => new PhoneNumber(invalidNumber);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*not a valid Egyptian phone number*");
    }

    [Theory]
    [InlineData("+20 1012345678")]      // Space in number
    [InlineData("+20-1012345678")]      // Hyphen in number
    [InlineData("+20(10)12345678")]     // Parentheses in number
    public void Constructor_WithFormattedNumber_ShouldThrowArgumentException(string formattedNumber)
    {
        // Arrange & Act
        var act = () => new PhoneNumber(formattedNumber);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Equality

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var phone1 = new PhoneNumber("+201012345678");
        var phone2 = new PhoneNumber("+201012345678");

        // Act & Assert
        phone1.Should().Be(phone2);
        (phone1 == phone2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var phone1 = new PhoneNumber("+201012345678");
        var phone2 = new PhoneNumber("+201112345678");

        // Act & Assert
        phone1.Should().NotBe(phone2);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ShouldReturnPhoneNumberValue()
    {
        // Arrange
        const string number = "+201012345678";
        var phone = new PhoneNumber(number);

        // Act
        var result = phone.ToString();

        // Assert
        result.Should().Be(number);
    }

    #endregion

    #region Egyptian Mobile Providers

    [Theory]
    [InlineData("+201012345678", "Vodafone")]  // 010 - Vodafone
    [InlineData("+201112345678", "Etisalat")]  // 011 - Etisalat
    [InlineData("+201212345678", "Orange")]     // 012 - Orange
    [InlineData("+201512345678", "WE")]         // 015 - WE
    public void Constructor_WithDifferentProviderPrefixes_ShouldAllBeValid(
        string number, string _provider)
    {
        // Arrange & Act
        var phone = new PhoneNumber(number);

        // Assert
        phone.Value.Should().Be(number);
    }

    #endregion
}
