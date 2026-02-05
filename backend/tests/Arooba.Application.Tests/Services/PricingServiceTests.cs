using Arooba.Application.Common.Models;
using Arooba.Application.Services;
using FluentAssertions;

namespace Arooba.Application.Tests.Services;

public class PricingServiceTests
{
    private readonly PricingService _sut = new();

    #region Business Rule Constants (for reference in assertions)

    private const decimal VatRate = 0.14m;
    private const decimal CooperativeFeeRate = 0.05m;
    private const decimal MvpFlatUpliftRate = 0.20m;
    private const decimal FragileUpliftRate = 0.25m;
    private const decimal JewelryUpliftRate = 0.15m;
    private const decimal MinimumFixedUplift = 15m;
    private const decimal LowPriceThreshold = 100m;
    private const decimal LowPriceFixedMarkup = 20m;
    private const decimal LogisticsSurcharge = 10m;
    private const int EscrowHoldDays = 14;

    #endregion

    #region Example Test Case: Ceramic Mug (Non-Legalized Vendor, Fragile)

    [Fact]
    public void CalculatePrice_CeramicMug_NonLegalizedVendor_FragileCategory_ShouldReturnCorrectBreakdown()
    {
        // Arrange - Ceramic mug, non-legalized vendor, home-decor-fragile category
        var input = new PricingInput(
            VendorBasePrice: 400m,
            CategoryId: "home-decor-fragile",
            IsVendorVatRegistered: false,
            IsNonLegalizedVendor: true,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Cooperative fee: 400 * 0.05 = 20
        result.CooperativeFee.Should().Be(20m);

        // Assert - Marketplace uplift: (400 + 20) * 0.25 = 105
        result.MarketplaceUplift.Should().Be(105m);

        // Assert - Logistics surcharge
        result.LogisticsSurcharge.Should().Be(10m);

        // Assert - Bucket A: vendor base price (no parent uplift)
        result.BucketA_VendorRevenue.Should().Be(400m);

        // Assert - Bucket B: 0 (vendor NOT VAT registered)
        result.BucketB_VendorVat.Should().Be(0m);

        // Assert - Bucket C: 20 + 105 + 10 = 135
        result.BucketC_AroobaRevenue.Should().Be(135m);

        // Assert - Bucket D: 135 * 0.14 = 18.90
        result.BucketD_AroobaVat.Should().Be(18.90m);

        // Assert - Final price: 400 + 0 + 135 + 18.90 = 553.90
        result.FinalPrice.Should().Be(553.90m);
    }

    #endregion

    #region Example Test Case: Silver Ring (Legalized Vendor, Jewelry)

    [Fact]
    public void CalculatePrice_SilverRing_LegalizedVendor_JewelryCategory_ShouldReturnCorrectBreakdown()
    {
        // Arrange - Silver ring, legalized vendor, jewelry-accessories category
        var input = new PricingInput(
            VendorBasePrice: 300m,
            CategoryId: "jewelry-accessories",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - No cooperative fee (legalized)
        result.CooperativeFee.Should().Be(0m);

        // Assert - Marketplace uplift: 300 * 0.15 = 45
        result.MarketplaceUplift.Should().Be(45m);

        // Assert - Logistics surcharge
        result.LogisticsSurcharge.Should().Be(10m);

        // Assert - Bucket A: 300
        result.BucketA_VendorRevenue.Should().Be(300m);

        // Assert - Bucket B: 300 * 0.14 = 42 (vendor IS VAT registered)
        result.BucketB_VendorVat.Should().Be(42m);

        // Assert - Bucket C: 0 + 45 + 10 = 55
        result.BucketC_AroobaRevenue.Should().Be(55m);

        // Assert - Bucket D: 55 * 0.14 = 7.70
        result.BucketD_AroobaVat.Should().Be(7.70m);

        // Assert - Final price: 300 + 42 + 55 + 7.70 = 404.70
        result.FinalPrice.Should().Be(404.70m);
    }

    #endregion

    #region Example Test Case: Cheap Soap (Non-Legalized, Under 100 EGP)

    [Fact]
    public void CalculatePrice_CheapSoap_NonLegalized_LowPrice_ShouldApplyMinimumAndLowPriceMarkup()
    {
        // Arrange - Cheap soap, 30 EGP, non-legalized, beauty-personal category
        var input = new PricingInput(
            VendorBasePrice: 30m,
            CategoryId: "beauty-personal",
            IsVendorVatRegistered: false,
            IsNonLegalizedVendor: true,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Cooperative fee: 30 * 0.05 = 1.50
        result.CooperativeFee.Should().Be(1.50m);

        // Assert - Marketplace uplift:
        //   Calculated: (30 + 1.50) * 0.20 = 6.30
        //   Min floor: max(6.30, 15) = 15
        //   Low price threshold: max(15, 20) = 20
        result.MarketplaceUplift.Should().Be(20m);

        // Assert - Logistics surcharge
        result.LogisticsSurcharge.Should().Be(10m);

        // Assert - Bucket A: 30
        result.BucketA_VendorRevenue.Should().Be(30m);

        // Assert - Bucket B: 0 (not VAT registered)
        result.BucketB_VendorVat.Should().Be(0m);

        // Assert - Bucket C: 1.50 + 20 + 10 = 31.50
        result.BucketC_AroobaRevenue.Should().Be(31.50m);

        // Assert - Bucket D: 31.50 * 0.14 = 4.41
        result.BucketD_AroobaVat.Should().Be(4.41m);

        // Assert - Final price: 30 + 0 + 31.50 + 4.41 = 65.91
        result.FinalPrice.Should().Be(65.91m);
    }

    #endregion

    #region Category-Specific Uplift Rates

    [Fact]
    public void CalculatePrice_JewelryCategory_ShouldApply15PercentUplift()
    {
        // Arrange
        var input = CreateStandardInput(vendorBasePrice: 200m, categoryId: "jewelry-accessories");

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - 200 * 0.15 = 30 (above min floor of 15)
        result.CommissionRate.Should().Be(JewelryUpliftRate);
        result.MarketplaceUplift.Should().Be(30m);
    }

    [Fact]
    public void CalculatePrice_FragileCategory_ShouldApply25PercentUplift()
    {
        // Arrange
        var input = CreateStandardInput(vendorBasePrice: 200m, categoryId: "home-decor-fragile");

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - 200 * 0.25 = 50 (above min floor of 15)
        result.CommissionRate.Should().Be(FragileUpliftRate);
        result.MarketplaceUplift.Should().Be(50m);
    }

    [Theory]
    [InlineData("beauty-personal")]
    [InlineData("clothing")]
    [InlineData("home-decor")]
    [InlineData("food-beverages")]
    [InlineData("unknown-category")]
    public void CalculatePrice_DefaultCategory_ShouldApply20PercentMvpUplift(string categoryId)
    {
        // Arrange
        var input = CreateStandardInput(vendorBasePrice: 200m, categoryId: categoryId);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - 200 * 0.20 = 40 (above min floor of 15)
        result.CommissionRate.Should().Be(MvpFlatUpliftRate);
        result.MarketplaceUplift.Should().Be(40m);
    }

    #endregion

    #region Non-Legalized Vendor (Cooperative Fee)

    [Fact]
    public void CalculatePrice_NonLegalizedVendor_ShouldAdd5PercentCooperativeFee()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 500m,
            CategoryId: "clothing",
            IsVendorVatRegistered: false,
            IsNonLegalizedVendor: true,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - 500 * 0.05 = 25
        result.CooperativeFee.Should().Be(25m);
    }

    [Fact]
    public void CalculatePrice_LegalizedVendor_ShouldHaveZeroCooperativeFee()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 500m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.CooperativeFee.Should().Be(0m);
    }

    [Fact]
    public void CalculatePrice_NonLegalizedVendor_CoopFeeIncludedInBucketC()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 200m,
            CategoryId: "clothing",
            IsVendorVatRegistered: false,
            IsNonLegalizedVendor: true,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Coop fee = 200 * 0.05 = 10
        // Marketplace uplift = (200 + 10) * 0.20 = 42
        // BucketC = 10 + 42 + 10 = 62
        result.BucketC_AroobaRevenue.Should().Be(
            result.CooperativeFee + result.MarketplaceUplift + result.LogisticsSurcharge);
    }

    #endregion

    #region Minimum Uplift Floor (15 EGP)

    [Fact]
    public void CalculatePrice_WhenCalculatedUpliftBelowMinimum_ShouldApplyMinimumFloor()
    {
        // Arrange - 60 EGP base, legalized, clothing (20%)
        // Calculated uplift: 60 * 0.20 = 12, but minimum is 15
        var input = new PricingInput(
            VendorBasePrice: 60m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Should be minimum 15, not calculated 12
        result.MarketplaceUplift.Should().Be(MinimumFixedUplift);
    }

    [Fact]
    public void CalculatePrice_WhenCalculatedUpliftAboveMinimum_ShouldUseCalculatedUplift()
    {
        // Arrange - 200 EGP base, legalized, clothing (20%)
        // Calculated uplift: 200 * 0.20 = 40 > 15 minimum
        var input = CreateStandardInput(vendorBasePrice: 200m, categoryId: "clothing");

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Should use calculated 40, not minimum 15
        result.MarketplaceUplift.Should().Be(40m);
    }

    #endregion

    #region Low Price Threshold (Under 100 EGP -> 20 EGP Fixed Markup)

    [Fact]
    public void CalculatePrice_UnderLowPriceThreshold_ShouldApply20EgpFixedMarkup()
    {
        // Arrange - 50 EGP, legalized, default category
        // Calculated uplift: 50 * 0.20 = 10
        // Min floor: max(10, 15) = 15
        // Low price threshold: max(15, 20) = 20
        var input = new PricingInput(
            VendorBasePrice: 50m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.MarketplaceUplift.Should().Be(LowPriceFixedMarkup);
    }

    [Fact]
    public void CalculatePrice_ExactlyAtLowPriceThreshold_ShouldNotApplyFixedMarkup()
    {
        // Arrange - Exactly 100 EGP (threshold is < 100, not <=)
        var input = new PricingInput(
            VendorBasePrice: 100m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - 100 * 0.20 = 20, no low price override since price is exactly 100
        result.MarketplaceUplift.Should().Be(20m);
    }

    [Fact]
    public void CalculatePrice_SlightlyUnderLowPriceThreshold_ShouldApplyFixedMarkup()
    {
        // Arrange - 99 EGP
        var input = new PricingInput(
            VendorBasePrice: 99m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - 99 * 0.20 = 19.80, min floor max(19.80, 15) = 19.80, low price max(19.80, 20) = 20
        result.MarketplaceUplift.Should().Be(20m);
    }

    #endregion

    #region Parent Vendor Uplift

    [Fact]
    public void CalculatePrice_WithFixedParentUplift_ShouldAddToBucketA()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 200m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: "fixed",
            ParentUpliftValue: 30m,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Bucket A = vendorBasePrice + parentUplift = 200 + 30 = 230
        result.ParentUpliftAmount.Should().Be(30m);
        result.BucketA_VendorRevenue.Should().Be(230m);
    }

    [Fact]
    public void CalculatePrice_WithPercentageParentUplift_ShouldCalculateCorrectly()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 200m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: "percentage",
            ParentUpliftValue: 10m,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Parent uplift = 200 * 10 / 100 = 20
        result.ParentUpliftAmount.Should().Be(20m);
        result.BucketA_VendorRevenue.Should().Be(220m);
    }

    [Fact]
    public void CalculatePrice_WithNoParentUplift_BucketAShouldEqualVendorBasePrice()
    {
        // Arrange
        var input = CreateStandardInput(vendorBasePrice: 200m);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.ParentUpliftAmount.Should().Be(0m);
        result.BucketA_VendorRevenue.Should().Be(200m);
    }

    [Fact]
    public void CalculatePrice_WithParentUplift_VatShouldApplyToFullBucketA()
    {
        // Arrange - VAT registered vendor with parent uplift
        var input = new PricingInput(
            VendorBasePrice: 200m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: "fixed",
            ParentUpliftValue: 50m,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - BucketA = 200 + 50 = 250, BucketB = 250 * 0.14 = 35
        result.BucketA_VendorRevenue.Should().Be(250m);
        result.BucketB_VendorVat.Should().Be(35m);
    }

    #endregion

    #region VAT Calculation

    [Fact]
    public void CalculatePrice_VatRegisteredVendor_ShouldApplyVatOnBucketA()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 1000m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - BucketB = 1000 * 0.14 = 140
        result.BucketB_VendorVat.Should().Be(140m);
        result.VatRate.Should().Be(VatRate);
    }

    [Fact]
    public void CalculatePrice_NonVatRegisteredVendor_ShouldHaveZeroBucketB()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 1000m,
            CategoryId: "clothing",
            IsVendorVatRegistered: false,
            IsNonLegalizedVendor: true,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.BucketB_VendorVat.Should().Be(0m);
    }

    [Fact]
    public void CalculatePrice_AroobaVat_ShouldAlwaysBeAppliedOnBucketC()
    {
        // Arrange - Even for legalized vendor, Arooba VAT (Bucket D) is always applied
        var input = new PricingInput(
            VendorBasePrice: 200m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - BucketD = BucketC * 0.14 and BucketD > 0
        result.BucketD_AroobaVat.Should().BeGreaterThan(0m);
        result.BucketD_AroobaVat.Should().Be(
            Math.Round(result.BucketC_AroobaRevenue * VatRate, 2));
    }

    [Fact]
    public void CalculatePrice_AroobaVat_ShouldAlsoApplyForNonLegalizedVendor()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 200m,
            CategoryId: "clothing",
            IsVendorVatRegistered: false,
            IsNonLegalizedVendor: true,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Bucket D should still be calculated even when Bucket B is 0
        result.BucketB_VendorVat.Should().Be(0m);
        result.BucketD_AroobaVat.Should().BeGreaterThan(0m);
    }

    [Fact]
    public void CalculatePrice_TotalVat_ShouldBeSumOfBucketBAndBucketD()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 500m,
            CategoryId: "jewelry-accessories",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.TotalVatAmount.Should().Be(result.BucketB_VendorVat + result.BucketD_AroobaVat);
    }

    #endregion

    #region Bucket Composition Verification

    [Fact]
    public void CalculatePrice_BucketA_ShouldEqualVendorBasePricePlusParentUplift()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 300m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: "fixed",
            ParentUpliftValue: 25m,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.BucketA_VendorRevenue.Should().Be(
            result.VendorBasePrice + result.ParentUpliftAmount);
    }

    [Theory]
    [InlineData(100, "clothing", false, true)]
    [InlineData(500, "jewelry-accessories", true, false)]
    [InlineData(30, "beauty-personal", false, true)]
    [InlineData(1000, "home-decor-fragile", true, false)]
    public void CalculatePrice_BucketC_ShouldEqualCoopFeePlusUpliftPlusLogistics(
        decimal basePrice, string categoryId, bool isVatRegistered, bool isNonLegalized)
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: basePrice,
            CategoryId: categoryId,
            IsVendorVatRegistered: isVatRegistered,
            IsNonLegalizedVendor: isNonLegalized,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.BucketC_AroobaRevenue.Should().Be(
            result.CooperativeFee + result.MarketplaceUplift + result.LogisticsSurcharge);
    }

    [Theory]
    [InlineData(400, "home-decor-fragile", false, true)]
    [InlineData(300, "jewelry-accessories", true, false)]
    [InlineData(30, "beauty-personal", false, true)]
    [InlineData(200, "clothing", true, false)]
    public void CalculatePrice_FinalPrice_ShouldEqualSumOfAllBuckets(
        decimal basePrice, string categoryId, bool isVatRegistered, bool isNonLegalized)
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: basePrice,
            CategoryId: categoryId,
            IsVendorVatRegistered: isVatRegistered,
            IsNonLegalizedVendor: isNonLegalized,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Final = A + B + C + D
        result.FinalPrice.Should().Be(
            result.BucketA_VendorRevenue
            + result.BucketB_VendorVat
            + result.BucketC_AroobaRevenue
            + result.BucketD_AroobaVat);
    }

    #endregion

    #region Custom Uplift Override

    [Fact]
    public void CalculatePrice_WithCustomUpliftOverride_ShouldBypassCategoryRate()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 200m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: 50m);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Should use override value, not calculated 200 * 0.20 = 40
        result.MarketplaceUplift.Should().Be(50m);
    }

    [Fact]
    public void CalculatePrice_WithCustomUpliftOverride_ShouldNotApplyMinFloorOrLowPriceMarkup()
    {
        // Arrange - Low price with custom override below both floors
        var input = new PricingInput(
            VendorBasePrice: 30m,
            CategoryId: "clothing",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: 5m);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Custom override of 5 should be used, not min 15 or low-price 20
        result.MarketplaceUplift.Should().Be(5m);
    }

    #endregion

    #region Logistics Surcharge

    [Fact]
    public void CalculatePrice_ShouldAlwaysInclude10EgpLogisticsSurcharge()
    {
        // Arrange
        var input = CreateStandardInput(vendorBasePrice: 500m);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.LogisticsSurcharge.Should().Be(LogisticsSurcharge);
    }

    #endregion

    #region Vendor Net Payout

    [Fact]
    public void CalculatePrice_VendorNetPayout_ShouldEqualVendorBasePrice()
    {
        // Arrange
        var input = CreateStandardInput(vendorBasePrice: 350m);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.VendorNetPayout.Should().Be(350m);
    }

    #endregion

    #region Effective Margin

    [Fact]
    public void CalculatePrice_EffectiveMarginPercent_ShouldBeBucketCOverFinalPriceTimes100()
    {
        // Arrange
        var input = CreateStandardInput(vendorBasePrice: 500m);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        var expectedMarginPercent = Math.Round(
            result.BucketC_AroobaRevenue / result.FinalPrice * 100, 2);
        result.EffectiveMarginPercent.Should().Be(expectedMarginPercent);
    }

    #endregion

    #region Shipping Fee Calculation

    [Fact]
    public void CalculateShippingFee_VolumetricGreaterThanActual_ShouldUseVolumetricWeight()
    {
        // Arrange - Large light box: volumetric = (50*40*30)/5000 = 12 kg > actual 2 kg
        var input = new ShippingFeeInput(
            ActualWeightKg: 2m,
            LengthCm: 50m,
            WidthCm: 40m,
            HeightCm: 30m,
            OriginZoneId: "zone-1",
            DestinationZoneId: "zone-2",
            RateCardId: "standard");

        // Act
        var result = _sut.CalculateShippingFee(input);

        // Assert
        result.VolumetricWeightKg.Should().Be(12m);
        result.ChargeableWeightKg.Should().Be(12m); // max(2, 12) = 12
        result.ActualWeightKg.Should().Be(2m);
    }

    [Fact]
    public void CalculateShippingFee_ActualGreaterThanVolumetric_ShouldUseActualWeight()
    {
        // Arrange - Heavy small item: volumetric = (10*10*10)/5000 = 0.2 kg < actual 5 kg
        var input = new ShippingFeeInput(
            ActualWeightKg: 5m,
            LengthCm: 10m,
            WidthCm: 10m,
            HeightCm: 10m,
            OriginZoneId: "zone-1",
            DestinationZoneId: "zone-2",
            RateCardId: "standard");

        // Act
        var result = _sut.CalculateShippingFee(input);

        // Assert
        result.VolumetricWeightKg.Should().Be(0.2m);
        result.ChargeableWeightKg.Should().Be(5m); // max(5, 0.2) = 5
    }

    [Fact]
    public void CalculateShippingFee_VolumetricDivisor_ShouldBe5000()
    {
        // Arrange - Known dimensions: (25 * 20 * 10) / 5000 = 1.0 kg
        var input = new ShippingFeeInput(
            ActualWeightKg: 0.5m,
            LengthCm: 25m,
            WidthCm: 20m,
            HeightCm: 10m,
            OriginZoneId: "zone-1",
            DestinationZoneId: "zone-2",
            RateCardId: "standard");

        // Act
        var result = _sut.CalculateShippingFee(input);

        // Assert
        result.VolumetricWeightKg.Should().Be(1.0m);
    }

    [Theory]
    [InlineData(0.5, 10, 10, 10, 0.5)]   // Actual > volumetric (0.2), use actual
    [InlineData(0.1, 50, 40, 30, 12)]     // Volumetric (12) > actual, use volumetric
    [InlineData(5, 50, 40, 30, 12)]        // Volumetric (12) > actual (5), use volumetric
    [InlineData(15, 50, 40, 30, 15)]       // Actual (15) > volumetric (12), use actual
    public void CalculateShippingFee_ShouldUseMaxOfActualAndVolumetric(
        decimal actualWeight, decimal length, decimal width, decimal height, decimal expectedChargeable)
    {
        // Arrange
        var input = new ShippingFeeInput(
            ActualWeightKg: actualWeight,
            LengthCm: length,
            WidthCm: width,
            HeightCm: height,
            OriginZoneId: "zone-1",
            DestinationZoneId: "zone-2",
            RateCardId: "standard");

        // Act
        var result = _sut.CalculateShippingFee(input);

        // Assert
        result.ChargeableWeightKg.Should().Be(expectedChargeable);
    }

    [Fact]
    public void CalculateShippingFee_CustomerFee_ShouldEqualTotalFee()
    {
        // Arrange
        var input = new ShippingFeeInput(
            ActualWeightKg: 2m,
            LengthCm: 20m,
            WidthCm: 15m,
            HeightCm: 10m,
            OriginZoneId: "zone-1",
            DestinationZoneId: "zone-2",
            RateCardId: "standard");

        // Act
        var result = _sut.CalculateShippingFee(input);

        // Assert
        result.CustomerShippingFee.Should().Be(result.TotalShippingFee);
    }

    #endregion

    #region Escrow Release

    [Fact]
    public void CalculateEscrowRelease_ShouldHave14DayHoldPeriod()
    {
        // Arrange
        var deliveryDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = _sut.CalculateEscrowRelease(deliveryDate);

        // Assert
        result.HoldDays.Should().Be(EscrowHoldDays);
        result.DeliveryDate.Should().Be(deliveryDate);
        result.ReleaseDate.Should().Be(deliveryDate.AddDays(14));
    }

    [Fact]
    public void CalculateEscrowRelease_ReleaseDateShouldBe14DaysAfterDelivery()
    {
        // Arrange
        var deliveryDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = _sut.CalculateEscrowRelease(deliveryDate);

        // Assert
        result.ReleaseDate.Should().Be(new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void CalculateEscrowRelease_OldDelivery_ShouldBeReleased()
    {
        // Arrange - Delivery was 30 days ago
        var deliveryDate = DateTime.UtcNow.AddDays(-30);

        // Act
        var result = _sut.CalculateEscrowRelease(deliveryDate);

        // Assert
        result.IsReleased.Should().BeTrue();
    }

    [Fact]
    public void CalculateEscrowRelease_RecentDelivery_ShouldNotBeReleased()
    {
        // Arrange - Delivery was 5 days ago
        var deliveryDate = DateTime.UtcNow.AddDays(-5);

        // Act
        var result = _sut.CalculateEscrowRelease(deliveryDate);

        // Assert
        result.IsReleased.Should().BeFalse();
    }

    [Theory]
    [InlineData(-15, true)]    // 15 days ago -> released (14 day hold passed)
    [InlineData(-14, true)]    // Exactly 14 days -> released (>= check)
    [InlineData(-13, false)]   // 13 days ago -> not yet released
    [InlineData(-1, false)]    // 1 day ago -> not yet released
    [InlineData(0, false)]     // Today -> not yet released
    public void CalculateEscrowRelease_VariousDeliveryDates_ShouldDetermineReleaseCorrectly(
        int daysAgo, bool expectedReleased)
    {
        // Arrange
        var deliveryDate = DateTime.UtcNow.AddDays(daysAgo);

        // Act
        var result = _sut.CalculateEscrowRelease(deliveryDate);

        // Assert
        result.IsReleased.Should().Be(expectedReleased);
    }

    #endregion

    #region Price Deviation Check

    [Fact]
    public void CheckPriceDeviation_WithinThreshold_ShouldNotFlag()
    {
        // Arrange - 10% deviation (within default 20% threshold)
        var productPrice = 110m;
        var categoryAvg = 100m;

        // Act
        var result = _sut.CheckPriceDeviation(productPrice, categoryAvg);

        // Assert
        result.IsFlagged.Should().BeFalse();
        result.DeviationPercent.Should().Be(0.10m);
    }

    [Fact]
    public void CheckPriceDeviation_ExceedingThreshold_ShouldFlag()
    {
        // Arrange - 30% deviation (exceeds default 20% threshold)
        var productPrice = 130m;
        var categoryAvg = 100m;

        // Act
        var result = _sut.CheckPriceDeviation(productPrice, categoryAvg);

        // Assert
        result.IsFlagged.Should().BeTrue();
        result.DeviationPercent.Should().Be(0.30m);
    }

    [Fact]
    public void CheckPriceDeviation_ExactlyAtThreshold_ShouldNotFlag()
    {
        // Arrange - Exactly 20% deviation (threshold is > not >=)
        var productPrice = 120m;
        var categoryAvg = 100m;

        // Act
        var result = _sut.CheckPriceDeviation(productPrice, categoryAvg);

        // Assert
        result.IsFlagged.Should().BeFalse();
        result.DeviationPercent.Should().Be(0.20m);
    }

    [Fact]
    public void CheckPriceDeviation_PriceBelowAverage_ShouldAlsoFlag()
    {
        // Arrange - Price 30% below average
        var productPrice = 70m;
        var categoryAvg = 100m;

        // Act
        var result = _sut.CheckPriceDeviation(productPrice, categoryAvg);

        // Assert
        result.IsFlagged.Should().BeTrue();
        result.DeviationPercent.Should().Be(0.30m);
    }

    [Fact]
    public void CheckPriceDeviation_SamePrice_ShouldNotFlag()
    {
        // Arrange
        var productPrice = 100m;
        var categoryAvg = 100m;

        // Act
        var result = _sut.CheckPriceDeviation(productPrice, categoryAvg);

        // Assert
        result.IsFlagged.Should().BeFalse();
        result.DeviationPercent.Should().Be(0m);
    }

    [Fact]
    public void CheckPriceDeviation_WithCustomThreshold_ShouldUseProvidedThreshold()
    {
        // Arrange - 15% deviation with 10% threshold -> should flag
        var productPrice = 115m;
        var categoryAvg = 100m;

        // Act
        var result = _sut.CheckPriceDeviation(productPrice, categoryAvg, threshold: 0.10m);

        // Assert
        result.IsFlagged.Should().BeTrue();
        result.Threshold.Should().Be(0.10m);
    }

    [Fact]
    public void CheckPriceDeviation_ZeroCategoryAverage_ShouldNotFlag()
    {
        // Arrange - Edge case: no category average
        var productPrice = 100m;
        var categoryAvg = 0m;

        // Act
        var result = _sut.CheckPriceDeviation(productPrice, categoryAvg);

        // Assert
        result.IsFlagged.Should().BeFalse();
        result.DeviationPercent.Should().Be(0m);
    }

    [Theory]
    [InlineData(200, 100, 0.20, true)]   // 100% deviation, 20% threshold -> flagged
    [InlineData(105, 100, 0.20, false)]  // 5% deviation, 20% threshold -> not flagged
    [InlineData(85, 100, 0.20, false)]   // 15% below, 20% threshold -> not flagged
    [InlineData(79, 100, 0.20, true)]    // 21% below, 20% threshold -> flagged
    [InlineData(121, 100, 0.20, true)]   // 21% above, 20% threshold -> flagged
    public void CheckPriceDeviation_VariousScenarios_ShouldFlagCorrectly(
        decimal productPrice, decimal categoryAvg, decimal threshold, bool expectedFlagged)
    {
        // Act
        var result = _sut.CheckPriceDeviation(productPrice, categoryAvg, threshold);

        // Assert
        result.IsFlagged.Should().Be(expectedFlagged);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CalculatePrice_VeryHighPrice_ShouldCalculateCorrectly()
    {
        // Arrange
        var input = new PricingInput(
            VendorBasePrice: 100_000m,
            CategoryId: "jewelry-accessories",
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert
        result.MarketplaceUplift.Should().Be(15_000m); // 100000 * 0.15
        result.BucketB_VendorVat.Should().Be(14_000m); // 100000 * 0.14
        result.FinalPrice.Should().Be(
            result.BucketA_VendorRevenue + result.BucketB_VendorVat
            + result.BucketC_AroobaRevenue + result.BucketD_AroobaVat);
    }

    [Fact]
    public void CalculatePrice_VerySmallPrice_ShouldApplyAllMinimumFloors()
    {
        // Arrange - 1 EGP item
        var input = new PricingInput(
            VendorBasePrice: 1m,
            CategoryId: "clothing",
            IsVendorVatRegistered: false,
            IsNonLegalizedVendor: true,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Low price threshold should apply: max(min_floor, 20) = 20
        result.MarketplaceUplift.Should().Be(LowPriceFixedMarkup);
        result.CooperativeFee.Should().Be(0.05m); // 1 * 0.05
    }

    #endregion

    #region Comprehensive Integration Test

    [Fact]
    public void CalculatePrice_FullScenario_NonLegalizedWithParentUplift_ShouldBeConsistent()
    {
        // Arrange - Non-legalized sub-vendor, parent has 10% uplift
        var input = new PricingInput(
            VendorBasePrice: 500m,
            CategoryId: "clothing",
            IsVendorVatRegistered: false,
            IsNonLegalizedVendor: true,
            ParentUpliftType: "percentage",
            ParentUpliftValue: 10m,
            CustomUpliftOverride: null);

        // Act
        var result = _sut.CalculatePrice(input);

        // Assert - Step by step:
        // Cooperative fee: 500 * 0.05 = 25
        result.CooperativeFee.Should().Be(25m);

        // Parent uplift: 500 * 10% = 50
        result.ParentUpliftAmount.Should().Be(50m);

        // priceAfterCoop = 500 + 25 = 525
        // Marketplace uplift: 525 * 0.20 = 105
        result.MarketplaceUplift.Should().Be(105m);

        // Bucket A = 500 + 50 = 550
        result.BucketA_VendorRevenue.Should().Be(550m);

        // Bucket B = 0 (not VAT registered)
        result.BucketB_VendorVat.Should().Be(0m);

        // Bucket C = 25 + 105 + 10 = 140
        result.BucketC_AroobaRevenue.Should().Be(140m);

        // Bucket D = 140 * 0.14 = 19.60
        result.BucketD_AroobaVat.Should().Be(19.60m);

        // Final = 550 + 0 + 140 + 19.60 = 709.60
        result.FinalPrice.Should().Be(709.60m);

        // Vendor gets base price
        result.VendorNetPayout.Should().Be(500m);
    }

    #endregion

    #region Helper Methods

    private static PricingInput CreateStandardInput(
        decimal vendorBasePrice = 200m,
        string categoryId = "clothing")
    {
        return new PricingInput(
            VendorBasePrice: vendorBasePrice,
            CategoryId: categoryId,
            IsVendorVatRegistered: true,
            IsNonLegalizedVendor: false,
            ParentUpliftType: null,
            ParentUpliftValue: null,
            CustomUpliftOverride: null);
    }

    #endregion
}
