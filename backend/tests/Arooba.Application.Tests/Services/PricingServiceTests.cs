using Arooba.Application.Common.Models;
using Arooba.Infrastructure.Services;
using FluentAssertions;

namespace Arooba.Application.Tests.Services;

/// <summary>
/// Tests for the Arooba Pricing Engine — the most critical business logic.
/// Validates the Additive Uplift Model and 5-Bucket Waterfall Split.
/// </summary>
public class PricingServiceTests
{
    private readonly PricingService _sut = new();

    // ──────────────────────────────────────────────
    // Test Case 1: Legalized vendor, silver ring (jewelry)
    // Base: 300 EGP, Category: jewelry-accessories (15%)
    // Expected: Bucket A=300, B=42, C=55, D=7.70, Final=404.70
    // ──────────────────────────────────────────────
    [Fact]
    public void CalculatePrice_LegalizedVendor_JewelryCategory_ShouldReturnCorrectBreakdown()
    {
        var input = new PricingInput
        {
            VendorBasePrice = 300m,
            CategoryId = "jewelry-accessories",
            IsVendorVatRegistered = true,
            IsNonLegalizedVendor = false,
        };

        var result = _sut.CalculatePrice(input);

        result.VendorBasePrice.Should().Be(300m);
        result.CooperativeFee.Should().Be(0m);
        result.MarketplaceUplift.Should().Be(45m);         // 300 * 0.15
        result.LogisticsSurcharge.Should().Be(10m);
        result.BucketA_VendorRevenue.Should().Be(300m);     // vendorBase + 0 parent
        result.BucketB_VendorVat.Should().Be(42m);          // 300 * 0.14
        result.BucketC_AroobaRevenue.Should().Be(55m);      // 0 + 45 + 10
        result.BucketD_AroobaVat.Should().Be(7.70m);        // 55 * 0.14
        result.FinalPrice.Should().Be(404.70m);             // A+B+C+D
    }

    // ──────────────────────────────────────────────
    // Test Case 2: Non-legalized vendor, ceramic mug (fragile)
    // Base: 400, Category: home-decor-fragile (25%)
    // Coop fee: 400 * 0.05 = 20
    // Uplift: (400+20) * 0.25 = 105
    // Expected: A=400, B=0, C=135, D=18.90, Final=553.90
    // ──────────────────────────────────────────────
    [Fact]
    public void CalculatePrice_NonLegalizedVendor_FragileCategory_ShouldIncludeCoopFee()
    {
        var input = new PricingInput
        {
            VendorBasePrice = 400m,
            CategoryId = "home-decor-fragile",
            IsVendorVatRegistered = false,
            IsNonLegalizedVendor = true,
        };

        var result = _sut.CalculatePrice(input);

        result.CooperativeFee.Should().Be(20m);             // 400 * 0.05
        result.MarketplaceUplift.Should().Be(105m);         // 420 * 0.25
        result.BucketA_VendorRevenue.Should().Be(400m);
        result.BucketB_VendorVat.Should().Be(0m);           // not VAT registered
        result.BucketC_AroobaRevenue.Should().Be(135m);     // 20 + 105 + 10
        result.BucketD_AroobaVat.Should().Be(18.90m);       // 135 * 0.14
        result.FinalPrice.Should().Be(553.90m);
    }

    // ──────────────────────────────────────────────
    // Test Case 3: Cheap soap (under 100 EGP threshold)
    // Base: 30, Category: beauty-personal (20%)
    // Non-legalized: coop fee = 30 * 0.05 = 1.50
    // Uplift: 31.50 * 0.20 = 6.30 → min(15) → max(20 for <100 items)
    // Expected: A=30, B=0, C=31.50, D=4.41, Final=65.91
    // ──────────────────────────────────────────────
    [Fact]
    public void CalculatePrice_CheapItem_ShouldApplyFixedMarkup()
    {
        var input = new PricingInput
        {
            VendorBasePrice = 30m,
            CategoryId = "beauty-personal",
            IsVendorVatRegistered = false,
            IsNonLegalizedVendor = true,
        };

        var result = _sut.CalculatePrice(input);

        result.CooperativeFee.Should().Be(1.50m);
        result.MarketplaceUplift.Should().Be(20m);          // fixed markup for <100 EGP
        result.BucketA_VendorRevenue.Should().Be(30m);
        result.BucketB_VendorVat.Should().Be(0m);
        result.BucketC_AroobaRevenue.Should().Be(31.50m);   // 1.50 + 20 + 10
        result.BucketD_AroobaVat.Should().Be(4.41m);        // 31.50 * 0.14
        result.FinalPrice.Should().Be(65.91m);
    }

    // ──────────────────────────────────────────────
    // Test Case 4: Minimum uplift floor (15 EGP)
    // ──────────────────────────────────────────────
    [Fact]
    public void CalculatePrice_LowUplift_ShouldApplyMinimumFloor()
    {
        // 50 EGP item in food category (12%): 50 * 0.12 = 6 → below 15 minimum
        var input = new PricingInput
        {
            VendorBasePrice = 50m,
            CategoryId = "food-essentials",
            IsVendorVatRegistered = false,
            IsNonLegalizedVendor = false,
        };

        var result = _sut.CalculatePrice(input);

        // Under 100 EGP threshold: max(15, 20) = 20
        result.MarketplaceUplift.Should().Be(20m);
    }

    // ──────────────────────────────────────────────
    // Test Case 5: Parent vendor uplift (fixed)
    // ──────────────────────────────────────────────
    [Fact]
    public void CalculatePrice_WithFixedParentUplift_ShouldAddToBucketA()
    {
        var input = new PricingInput
        {
            VendorBasePrice = 200m,
            CategoryId = "leather-goods",
            IsVendorVatRegistered = true,
            IsNonLegalizedVendor = false,
            ParentUpliftType = "fixed",
            ParentUpliftValue = 30m,
        };

        var result = _sut.CalculatePrice(input);

        result.ParentVendorUplift.Should().Be(30m);
        result.BucketA_VendorRevenue.Should().Be(230m);     // 200 + 30
        result.BucketB_VendorVat.Should().Be(32.20m);       // 230 * 0.14
    }

    // ──────────────────────────────────────────────
    // Test Case 6: Parent vendor uplift (percentage)
    // ──────────────────────────────────────────────
    [Fact]
    public void CalculatePrice_WithPercentageParentUplift_ShouldCalculateCorrectly()
    {
        var input = new PricingInput
        {
            VendorBasePrice = 500m,
            CategoryId = "home-decor-textiles",
            IsVendorVatRegistered = false,
            IsNonLegalizedVendor = false,
            ParentUpliftType = "percentage",
            ParentUpliftValue = 0.10m, // 10%
        };

        var result = _sut.CalculatePrice(input);

        result.ParentVendorUplift.Should().Be(50m);         // 500 * 0.10
        result.BucketA_VendorRevenue.Should().Be(550m);     // 500 + 50
    }

    // ──────────────────────────────────────────────
    // Test Case 7: VAT always on Arooba revenue (Bucket D)
    // ──────────────────────────────────────────────
    [Fact]
    public void CalculatePrice_AroobaVat_ShouldAlwaysApply()
    {
        var input = new PricingInput
        {
            VendorBasePrice = 300m,
            CategoryId = "fashion-apparel",
            IsVendorVatRegistered = false,
            IsNonLegalizedVendor = false,
        };

        var result = _sut.CalculatePrice(input);

        // Even though vendor is not VAT registered, Arooba VAT always applies
        result.BucketB_VendorVat.Should().Be(0m);
        result.BucketD_AroobaVat.Should().BeGreaterThan(0m);
    }

    // ──────────────────────────────────────────────
    // Shipping Fee Tests
    // ──────────────────────────────────────────────
    [Fact]
    public void CalculateShippingFee_VolumetricHeavier_ShouldUseVolumetric()
    {
        var input = new ShippingFeeInput
        {
            ActualWeightKg = 0.5m,
            DimensionL = 50,
            DimensionW = 40,
            DimensionH = 30,
            FromZoneId = "cairo",
            ToZoneId = "cairo",
            BaseRate = 35,
            PerKgRate = 5,
        };

        var result = _sut.CalculateShippingFee(input);

        // Volumetric: (50*40*30)/5000 = 12
        result.VolumetricWeight.Should().Be(12m);
        result.ChargeableWeight.Should().Be(12m);  // > actual 0.5
    }

    [Fact]
    public void CalculateShippingFee_ActualHeavier_ShouldUseActual()
    {
        var input = new ShippingFeeInput
        {
            ActualWeightKg = 5m,
            DimensionL = 10,
            DimensionW = 10,
            DimensionH = 10,
            FromZoneId = "cairo",
            ToZoneId = "alexandria",
            BaseRate = 45,
            PerKgRate = 7,
        };

        var result = _sut.CalculateShippingFee(input);

        // Volumetric: (10*10*10)/5000 = 0.2 → actual 5 is heavier
        result.ChargeableWeight.Should().Be(5m);
        // Fee: 45 + max(0, 5-1)*7 = 45 + 28 = 73
        result.TotalFee.Should().Be(73m);
    }

    // ──────────────────────────────────────────────
    // Escrow Tests
    // ──────────────────────────────────────────────
    [Fact]
    public void CalculateEscrowRelease_FreshDelivery_ShouldNotBeReleasable()
    {
        var deliveryDate = DateTime.UtcNow.AddDays(-5);
        var result = _sut.CalculateEscrowRelease(deliveryDate);

        result.IsReleasable.Should().BeFalse();
        result.DaysRemaining.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateEscrowRelease_After14Days_ShouldBeReleasable()
    {
        var deliveryDate = DateTime.UtcNow.AddDays(-15);
        var result = _sut.CalculateEscrowRelease(deliveryDate);

        result.IsReleasable.Should().BeTrue();
        result.DaysRemaining.Should().Be(0);
    }

    // ──────────────────────────────────────────────
    // Price Deviation Tests
    // ──────────────────────────────────────────────
    [Fact]
    public void CheckPriceDeviation_WithinThreshold_ShouldNotFlag()
    {
        var result = _sut.CheckPriceDeviation(110m, 100m);
        result.IsFlagged.Should().BeFalse();
        result.Direction.Should().Be("normal");
    }

    [Fact]
    public void CheckPriceDeviation_Above20Percent_ShouldFlagAbove()
    {
        var result = _sut.CheckPriceDeviation(150m, 100m);
        result.IsFlagged.Should().BeTrue();
        result.Direction.Should().Be("above");
    }

    [Fact]
    public void CheckPriceDeviation_Below20Percent_ShouldFlagBelow()
    {
        var result = _sut.CheckPriceDeviation(70m, 100m);
        result.IsFlagged.Should().BeTrue();
        result.Direction.Should().Be("below");
    }
}
