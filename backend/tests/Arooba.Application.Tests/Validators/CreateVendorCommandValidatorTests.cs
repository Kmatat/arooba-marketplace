using Arooba.Application.Features.Vendors.Commands;
using Arooba.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Arooba.Application.Tests.Validators;

public class CreateVendorCommandValidatorTests
{
    private readonly CreateVendorCommandValidator _validator = new();

    #region Valid Command

    [Fact]
    public void Validate_ValidCommand_ShouldHaveNoErrors()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region BusinessNameAr

    [Fact]
    public void Validate_EmptyBusinessNameAr_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { BusinessNameAr = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.BusinessNameAr)
            .WithErrorMessage("Arabic business name is required.");
    }

    [Fact]
    public void Validate_BusinessNameAr_ExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { BusinessNameAr = new string('ا', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.BusinessNameAr)
            .WithErrorMessage("Arabic business name must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_BusinessNameAr_ExactlyMaxLength_ShouldBeValid()
    {
        // Arrange
        var command = CreateValidCommand() with { BusinessNameAr = new string('ا', 200) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.BusinessNameAr);
    }

    #endregion

    #region BusinessNameEn

    [Fact]
    public void Validate_EmptyBusinessNameEn_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { BusinessNameEn = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.BusinessNameEn)
            .WithErrorMessage("English business name is required.");
    }

    [Fact]
    public void Validate_BusinessNameEn_ExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { BusinessNameEn = new string('A', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.BusinessNameEn)
            .WithErrorMessage("English business name must not exceed 200 characters.");
    }

    #endregion

    #region PhoneNumber

    [Fact]
    public void Validate_EmptyPhoneNumber_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { PhoneNumber = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.PhoneNumber)
            .WithErrorMessage("Phone number is required.");
    }

    [Theory]
    [InlineData("+201012345678")]
    [InlineData("+201112345678")]
    [InlineData("+201212345678")]
    [InlineData("+201512345678")]
    public void Validate_ValidEgyptianPhoneNumber_ShouldNotHaveError(string phoneNumber)
    {
        // Arrange
        var command = CreateValidCommand() with { PhoneNumber = phoneNumber };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.PhoneNumber);
    }

    [Theory]
    [InlineData("01012345678")]          // Missing +20
    [InlineData("+20012345678")]         // Invalid prefix after +20
    [InlineData("+971501234567")]        // Non-Egyptian
    [InlineData("12345")]               // Too short
    [InlineData("+201012345678901")]     // Too long
    public void Validate_InvalidPhoneNumber_ShouldHaveError(string phoneNumber)
    {
        // Arrange
        var command = CreateValidCommand() with { PhoneNumber = phoneNumber };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.PhoneNumber)
            .WithErrorMessage("Phone number must be a valid Egyptian number in +201XXXXXXXXX format.");
    }

    #endregion

    #region Email

    [Fact]
    public void Validate_EmptyEmail_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { Email = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData("vendor@example.com")]
    [InlineData("test.user@arooba.eg")]
    [InlineData("name+tag@domain.co.uk")]
    public void Validate_ValidEmail_ShouldNotHaveError(string email)
    {
        // Arrange
        var command = CreateValidCommand() with { Email = email };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public void Validate_InvalidEmail_ShouldHaveError(string email)
    {
        // Arrange
        var command = CreateValidCommand() with { Email = email };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    #endregion

    #region NationalId

    [Fact]
    public void Validate_EmptyNationalId_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { NationalId = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NationalId)
            .WithErrorMessage("National ID is required.");
    }

    [Fact]
    public void Validate_ValidNationalId_14Digits_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { NationalId = "29001011234567" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.NationalId);
    }

    [Theory]
    [InlineData("1234567890123")]       // 13 digits - too short
    [InlineData("123456789012345")]     // 15 digits - too long
    [InlineData("2900101123456A")]      // Contains letter
    [InlineData("29001011234 67")]      // Contains space
    public void Validate_InvalidNationalId_ShouldHaveError(string nationalId)
    {
        // Arrange
        var command = CreateValidCommand() with { NationalId = nationalId };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NationalId)
            .WithErrorMessage("National ID must be exactly 14 digits.");
    }

    #endregion

    #region VendorType

    [Theory]
    [InlineData(VendorType.Legalized)]
    [InlineData(VendorType.NonLegalized)]
    public void Validate_ValidVendorType_ShouldNotHaveError(VendorType vendorType)
    {
        // Arrange
        var command = CreateValidCommand() with { VendorType = vendorType };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.VendorType);
    }

    [Fact]
    public void Validate_InvalidVendorType_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { VendorType = (VendorType)99 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.VendorType);
    }

    #endregion

    #region City

    [Fact]
    public void Validate_EmptyCity_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { City = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.City)
            .WithErrorMessage("City is required.");
    }

    [Fact]
    public void Validate_ValidCity_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { City = "Cairo" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.City);
    }

    #endregion

    #region GovernorateId

    [Fact]
    public void Validate_EmptyGovernorateId_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { GovernorateId = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.GovernorateId)
            .WithErrorMessage("Governorate is required.");
    }

    #endregion

    #region Multiple Validation Errors

    [Fact]
    public void Validate_MultipleInvalidFields_ShouldReturnMultipleErrors()
    {
        // Arrange
        var command = new CreateVendorCommand
        {
            BusinessNameAr = string.Empty,
            BusinessNameEn = string.Empty,
            PhoneNumber = string.Empty,
            Email = string.Empty,
            NationalId = string.Empty,
            City = string.Empty,
            GovernorateId = string.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.BusinessNameAr);
        result.ShouldHaveValidationErrorFor(c => c.BusinessNameEn);
        result.ShouldHaveValidationErrorFor(c => c.PhoneNumber);
        result.ShouldHaveValidationErrorFor(c => c.Email);
        result.ShouldHaveValidationErrorFor(c => c.NationalId);
        result.ShouldHaveValidationErrorFor(c => c.City);
        result.ShouldHaveValidationErrorFor(c => c.GovernorateId);
    }

    #endregion

    #region Helpers

    private static CreateVendorCommand CreateValidCommand()
    {
        return new CreateVendorCommand
        {
            BusinessNameAr = "متجر عروبة للحرف اليدوية",
            BusinessNameEn = "Arooba Crafts Store",
            PhoneNumber = "+201012345678",
            Email = "vendor@arooba.eg",
            NationalId = "29001011234567",
            VendorType = VendorType.Legalized,
            City = "Cairo",
            GovernorateId = "gov-cairo"
        };
    }

    #endregion
}
