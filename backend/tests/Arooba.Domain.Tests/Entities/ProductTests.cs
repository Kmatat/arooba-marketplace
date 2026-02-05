using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentAssertions;

namespace Arooba.Domain.Tests.Entities;

public class ProductTests
{
    #region Volumetric Weight Calculation

    [Fact]
    public void CalculateVolumetricWeight_StandardBox_ShouldReturnCorrectWeight()
    {
        // Arrange
        var product = new Product
        {
            LengthCm = 50m,
            WidthCm = 40m,
            HeightCm = 30m
        };

        // Act
        var volumetricWeight = product.CalculateVolumetricWeight();

        // Assert
        // (50 * 40 * 30) / 5000 = 60000 / 5000 = 12
        volumetricWeight.Should().Be(12m);
    }

    [Theory]
    [InlineData(10, 10, 10, 0.2)]       // Small cube: 1000/5000 = 0.2 kg
    [InlineData(100, 50, 50, 50)]        // Large box: 250000/5000 = 50 kg
    [InlineData(20, 15, 10, 0.6)]        // Shoebox: 3000/5000 = 0.6 kg
    [InlineData(30, 20, 15, 1.8)]        // Medium box: 9000/5000 = 1.8 kg
    [InlineData(1, 1, 1, 0.0002)]        // Tiny item: 1/5000 = 0.0002 kg
    public void CalculateVolumetricWeight_VariousDimensions_ShouldReturnCorrectWeight(
        decimal length, decimal width, decimal height, decimal expectedWeight)
    {
        // Arrange
        var product = new Product
        {
            LengthCm = length,
            WidthCm = width,
            HeightCm = height
        };

        // Act
        var volumetricWeight = product.CalculateVolumetricWeight();

        // Assert
        volumetricWeight.Should().Be(expectedWeight);
    }

    [Fact]
    public void CalculateVolumetricWeight_WithZeroDimension_ShouldReturnZero()
    {
        // Arrange
        var product = new Product
        {
            LengthCm = 50m,
            WidthCm = 0m,
            HeightCm = 30m
        };

        // Act
        var volumetricWeight = product.CalculateVolumetricWeight();

        // Assert
        volumetricWeight.Should().Be(0m);
    }

    #endregion

    #region Status Transitions - SubmitForReview

    [Fact]
    public void SubmitForReview_FromDraft_ShouldTransitionToPendingReview()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.Draft };

        // Act
        var result = product.SubmitForReview();

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.PendingReview);
    }

    [Theory]
    [InlineData(ProductStatus.PendingReview)]
    [InlineData(ProductStatus.Active)]
    [InlineData(ProductStatus.Paused)]
    [InlineData(ProductStatus.Rejected)]
    public void SubmitForReview_FromNonDraftStatus_ShouldFail(ProductStatus invalidStatus)
    {
        // Arrange
        var product = new Product { Status = invalidStatus };

        // Act
        var result = product.SubmitForReview();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only draft products");
        product.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region Status Transitions - Approve

    [Fact]
    public void Approve_FromPendingReview_ShouldTransitionToActive()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.PendingReview };

        // Act
        var result = product.Approve();

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Theory]
    [InlineData(ProductStatus.Draft)]
    [InlineData(ProductStatus.Active)]
    [InlineData(ProductStatus.Paused)]
    [InlineData(ProductStatus.Rejected)]
    public void Approve_FromNonPendingReviewStatus_ShouldFail(ProductStatus invalidStatus)
    {
        // Arrange
        var product = new Product { Status = invalidStatus };

        // Act
        var result = product.Approve();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only products pending review");
        product.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region Status Transitions - Reject

    [Fact]
    public void Reject_FromPendingReview_ShouldTransitionToRejected()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.PendingReview };

        // Act
        var result = product.Reject();

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Rejected);
    }

    [Theory]
    [InlineData(ProductStatus.Draft)]
    [InlineData(ProductStatus.Active)]
    [InlineData(ProductStatus.Paused)]
    [InlineData(ProductStatus.Rejected)]
    public void Reject_FromNonPendingReviewStatus_ShouldFail(ProductStatus invalidStatus)
    {
        // Arrange
        var product = new Product { Status = invalidStatus };

        // Act
        var result = product.Reject();

        // Assert
        result.IsFailure.Should().BeTrue();
        product.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region Status Transitions - Pause

    [Fact]
    public void Pause_FromActive_ShouldTransitionToPaused()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.Active };

        // Act
        var result = product.Pause();

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Paused);
    }

    [Theory]
    [InlineData(ProductStatus.Draft)]
    [InlineData(ProductStatus.PendingReview)]
    [InlineData(ProductStatus.Paused)]
    [InlineData(ProductStatus.Rejected)]
    public void Pause_FromNonActiveStatus_ShouldFail(ProductStatus invalidStatus)
    {
        // Arrange
        var product = new Product { Status = invalidStatus };

        // Act
        var result = product.Pause();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only active products");
        product.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region Status Transitions - Unpause

    [Fact]
    public void Unpause_FromPaused_ShouldTransitionToActive()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.Paused };

        // Act
        var result = product.Unpause();

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Theory]
    [InlineData(ProductStatus.Draft)]
    [InlineData(ProductStatus.PendingReview)]
    [InlineData(ProductStatus.Active)]
    [InlineData(ProductStatus.Rejected)]
    public void Unpause_FromNonPausedStatus_ShouldFail(ProductStatus invalidStatus)
    {
        // Arrange
        var product = new Product { Status = invalidStatus };

        // Act
        var result = product.Unpause();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only paused products");
        product.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region Full Lifecycle

    [Fact]
    public void FullLifecycle_DraftToActiveToActiveAfterPause_ShouldSucceed()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.Draft };

        // Act & Assert - Draft -> PendingReview
        product.SubmitForReview().IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.PendingReview);

        // Act & Assert - PendingReview -> Active
        product.Approve().IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Active);

        // Act & Assert - Active -> Paused
        product.Pause().IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Paused);

        // Act & Assert - Paused -> Active
        product.Unpause().IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void FullLifecycle_DraftToRejected_ShouldSucceed()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.Draft };

        // Act & Assert - Draft -> PendingReview
        product.SubmitForReview().IsSuccess.Should().BeTrue();

        // Act & Assert - PendingReview -> Rejected
        product.Reject().IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Rejected);
    }

    #endregion

    #region Default Values

    [Fact]
    public void NewProduct_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        product.Status.Should().Be(ProductStatus.Draft);
        product.StockMode.Should().Be(StockMode.ReadyStock);
        product.Name.Should().BeEmpty();
        product.IsFragile.Should().BeFalse();
    }

    #endregion
}
