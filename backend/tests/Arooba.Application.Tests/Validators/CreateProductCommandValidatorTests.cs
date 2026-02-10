using Arooba.Application.Features.Products.Commands;
using Arooba.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

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
        var command = CreateValidCommand() with { TitleAr = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TitleAr)
            .WithErrorMessage("Product name is required.");
    }

    [Fact]
    public void Validate_Name_ExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { TitleAr = new string('A', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TitleAr)
            .WithErrorMessage("Product name must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_Name_ExactlyMaxLength_ShouldBeValid()
    {
        // Arrange
        var command = CreateValidCommand() with { TitleAr = new string('A', 200) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.TitleAr);
    }

    [Theory]
    [InlineData("Handmade Ceramic Mug")]
    [InlineData("طقم فناجين قهوة")]
    [InlineData("A")]
    public void Validate_ValidNames_ShouldNotHaveError(string name)
    {
        // Arrange
        var command = CreateValidCommand() with { TitleAr = name };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.TitleAr);
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

    #region SellingPrice

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ZeroOrNegativePrice_ShouldHaveError(decimal price)
    {
        // Arrange
        var command = CreateValidCommand() with { SellingPrice = price };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.SellingPrice)
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
        var command = CreateValidCommand() with { SellingPrice = price };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.SellingPrice);
    }

    #endregion

    #region CategoryId

    [Fact]
    public void Validate_EmptyCategoryId_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { CategoryId = 1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CategoryId)
            .WithErrorMessage("Category is required.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Validate_ValidCategoryIds_ShouldNotHaveError(int categoryId)
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
        var command = CreateValidCommand() with { DimensionL = length };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.DimensionL)
            .WithErrorMessage("Length must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_ZeroOrNegativeWidth_ShouldHaveError(decimal width)
    {
        // Arrange
        var command = CreateValidCommand() with { DimensionW = width };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.DimensionW)
            .WithErrorMessage("Width must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_ZeroOrNegativeHeight_ShouldHaveError(decimal height)
    {
        // Arrange
        var command = CreateValidCommand() with { DimensionH = height };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.DimensionH)
            .WithErrorMessage("Height must be greater than zero.");
    }

    [Fact]
    public void Validate_ValidDimensions_ShouldNotHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            WeightKg = 0.5m,
            DimensionL = 10m,
            DimensionW = 10m,
            DimensionH = 10m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.WeightKg);
        result.ShouldNotHaveValidationErrorFor(c => c.DimensionL);
        result.ShouldNotHaveValidationErrorFor(c => c.DimensionW);
        result.ShouldNotHaveValidationErrorFor(c => c.DimensionH);
    }

    #endregion

    #region StockQuantity

    [Fact]
    public void Validate_NegativeStockQuantity_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { QuantityAvailable = -1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.QuantityAvailable)
            .WithErrorMessage("Stock quantity cannot be negative.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    public void Validate_ZeroOrPositiveStockQuantity_ShouldNotHaveError(int quantity)
    {
        // Arrange
        var command = CreateValidCommand() with { QuantityAvailable = quantity };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.QuantityAvailable);
    }

    #endregion

    #region Multiple Validation Errors

    [Fact]
    public void Validate_AllFieldsInvalid_ShouldReturnMultipleErrors()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            TitleAr = string.Empty,
            CategoryId = 0,
            CostPrice = 0,
            WeightKg = 0,
            DimensionL = 0,
            DimensionW = 0,
            DimensionH = 0,
            QuantityAvailable = -1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TitleAr);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
        result.ShouldHaveValidationErrorFor(c => c.CostPrice);
        result.ShouldHaveValidationErrorFor(c => c.WeightKg);
        result.ShouldHaveValidationErrorFor(c => c.DimensionW);
        result.ShouldHaveValidationErrorFor(c => c.DimensionL);
        result.ShouldHaveValidationErrorFor(c => c.DimensionH);
        result.ShouldHaveValidationErrorFor(c => c.QuantityAvailable);
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
            TitleAr = "Handmade Ceramic Mug",
            Description = "A beautifully crafted ceramic mug made by Egyptian artisans.",
            CategoryId = 1,
            CostPrice = 150m,
            StockMode = StockMode.ReadyStock,
            QuantityAvailable = 50,
            WeightKg = 0.4m,
            DimensionW= 12m,
            DimensionL= 10m,
            DimensionH= 10m,
            IsLocalOnly = true
        };
    }

    #endregion
}
