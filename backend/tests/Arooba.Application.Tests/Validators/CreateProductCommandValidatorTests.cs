using Arooba.Application.Features.Products.Commands;
using Arooba.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Arooba.Application.Tests.Validators;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

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

    #region Name

    [Fact]
    public void Validate_EmptyName_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { Name = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Product name is required.");
    }

    [Fact]
    public void Validate_Name_ExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { Name = new string('A', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Product name must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_Name_ExactlyMaxLength_ShouldBeValid()
    {
        // Arrange
        var command = CreateValidCommand() with { Name = new string('A', 200) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }

    [Theory]
    [InlineData("Handmade Ceramic Mug")]
    [InlineData("طقم فناجين قهوة")]
    [InlineData("A")]
    public void Validate_ValidNames_ShouldNotHaveError(string name)
    {
        // Arrange
        var command = CreateValidCommand() with { Name = name };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }

    #endregion

    #region Description

    [Fact]
    public void Validate_NullDescription_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { Description = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void Validate_EmptyDescription_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { Description = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void Validate_Description_ExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { Description = new string('A', 2001) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Description)
            .WithErrorMessage("Description must not exceed 2000 characters.");
    }

    [Fact]
    public void Validate_Description_ExactlyMaxLength_ShouldBeValid()
    {
        // Arrange
        var command = CreateValidCommand() with { Description = new string('A', 2000) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Description);
    }

    #endregion

    #region VendorBasePrice

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ZeroOrNegativePrice_ShouldHaveError(decimal price)
    {
        // Arrange
        var command = CreateValidCommand() with { VendorBasePrice = price };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.VendorBasePrice)
            .WithErrorMessage("Vendor base price must be greater than zero.");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(99999.99)]
    public void Validate_PositivePrice_ShouldNotHaveError(decimal price)
    {
        // Arrange
        var command = CreateValidCommand() with { VendorBasePrice = price };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.VendorBasePrice);
    }

    #endregion

    #region CategoryId

    [Fact]
    public void Validate_EmptyCategoryId_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { CategoryId = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CategoryId)
            .WithErrorMessage("Category is required.");
    }

    [Theory]
    [InlineData("jewelry-accessories")]
    [InlineData("home-decor-fragile")]
    [InlineData("beauty-personal")]
    [InlineData("clothing")]
    public void Validate_ValidCategoryIds_ShouldNotHaveError(string categoryId)
    {
        // Arrange
        var command = CreateValidCommand() with { CategoryId = categoryId };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.CategoryId);
    }

    #endregion

    #region Weight and Dimensions

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ZeroOrNegativeWeight_ShouldHaveError(decimal weight)
    {
        // Arrange
        var command = CreateValidCommand() with { WeightKg = weight };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.WeightKg)
            .WithErrorMessage("Weight must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_ZeroOrNegativeLength_ShouldHaveError(decimal length)
    {
        // Arrange
        var command = CreateValidCommand() with { LengthCm = length };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.LengthCm)
            .WithErrorMessage("Length must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_ZeroOrNegativeWidth_ShouldHaveError(decimal width)
    {
        // Arrange
        var command = CreateValidCommand() with { WidthCm = width };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.WidthCm)
            .WithErrorMessage("Width must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_ZeroOrNegativeHeight_ShouldHaveError(decimal height)
    {
        // Arrange
        var command = CreateValidCommand() with { HeightCm = height };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.HeightCm)
            .WithErrorMessage("Height must be greater than zero.");
    }

    [Fact]
    public void Validate_ValidDimensions_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            WeightKg = 0.5m,
            LengthCm = 10m,
            WidthCm = 10m,
            HeightCm = 10m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.WeightKg);
        result.ShouldNotHaveValidationErrorFor(c => c.LengthCm);
        result.ShouldNotHaveValidationErrorFor(c => c.WidthCm);
        result.ShouldNotHaveValidationErrorFor(c => c.HeightCm);
    }

    #endregion

    #region StockQuantity

    [Fact]
    public void Validate_NegativeStockQuantity_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { StockQuantity = -1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StockQuantity)
            .WithErrorMessage("Stock quantity cannot be negative.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    public void Validate_ZeroOrPositiveStockQuantity_ShouldNotHaveError(int quantity)
    {
        // Arrange
        var command = CreateValidCommand() with { StockQuantity = quantity };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.StockQuantity);
    }

    #endregion

    #region Multiple Validation Errors

    [Fact]
    public void Validate_AllFieldsInvalid_ShouldReturnMultipleErrors()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = string.Empty,
            CategoryId = string.Empty,
            VendorBasePrice = 0,
            WeightKg = 0,
            LengthCm = 0,
            WidthCm = 0,
            HeightCm = 0,
            StockQuantity = -1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
        result.ShouldHaveValidationErrorFor(c => c.VendorBasePrice);
        result.ShouldHaveValidationErrorFor(c => c.WeightKg);
        result.ShouldHaveValidationErrorFor(c => c.LengthCm);
        result.ShouldHaveValidationErrorFor(c => c.WidthCm);
        result.ShouldHaveValidationErrorFor(c => c.HeightCm);
        result.ShouldHaveValidationErrorFor(c => c.StockQuantity);
    }

    #endregion

    #region StockMode

    [Theory]
    [InlineData(StockMode.ReadyStock)]
    [InlineData(StockMode.MadeToOrder)]
    public void Validate_ValidStockMode_ShouldNotHaveError(StockMode stockMode)
    {
        // Arrange
        var command = CreateValidCommand() with { StockMode = stockMode };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Helpers

    private static CreateProductCommand CreateValidCommand()
    {
        return new CreateProductCommand
        {
            Name = "Handmade Ceramic Mug",
            Description = "A beautifully crafted ceramic mug made by Egyptian artisans.",
            CategoryId = "home-decor",
            VendorBasePrice = 150m,
            StockMode = StockMode.ReadyStock,
            StockQuantity = 50,
            WeightKg = 0.4m,
            LengthCm = 12m,
            WidthCm = 10m,
            HeightCm = 10m,
            IsFragile = true
        };
    }

    #endregion
}
