using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arooba.Infrastructure.Persistence;

/// <summary>
/// Provides seed data for the Arooba Marketplace database.
/// Populates shipping zones, product categories, rate cards, and a super admin user
/// when the database is first created or migrated.
/// </summary>
public static class AroobaDbContextSeed
{
    /// <summary>
    /// Seeds the database with initial reference data required for the marketplace to operate.
    /// This method is idempotent and can be called multiple times safely.
    /// </summary>
    /// <param name="context">The database context to seed.</param>
    /// <param name="logger">The logger for recording seed operations.</param>
    /// <returns>A task representing the asynchronous seed operation.</returns>
    public static async Task SeedAsync(AroobaDbContext context, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            await SeedShippingZonesAsync(context, logger);
            await SeedProductCategoriesAsync(context, logger);
            await SeedRateCardsAsync(context, logger);
            await SeedSuperAdminAsync(context, logger);

            await context.SaveChangesAsync();
            logger.LogInformation("Arooba database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the Arooba database");
            throw;
        }
    }

    /// <summary>
    /// Seeds the 6 Egyptian shipping zones with their Arabic names and covered cities.
    /// Zones: Cairo, Alexandria, Delta, Upper Egypt, Canal, Sinai.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    private static async Task SeedShippingZonesAsync(AroobaDbContext context, ILogger logger)
    {
        if (await context.ShippingZones.AnyAsync())
        {
            logger.LogInformation("Shipping zones already seeded, skipping");
            return;
        }

        var zones = new List<ShippingZone>
        {
            new()
            {
                Id = "cairo",
                Name = "Greater Cairo",
                NameAr = "\u0627\u0644\u0642\u0627\u0647\u0631\u0629 \u0627\u0644\u0643\u0628\u0631\u0649",
                CitiesCovered =
                [
                    "\u0627\u0644\u0642\u0627\u0647\u0631\u0629",
                    "\u0627\u0644\u062c\u064a\u0632\u0629",
                    "\u0627\u0644\u0642\u0644\u064a\u0648\u0628\u064a\u0629",
                    "\u0633\u0627\u062f\u0633 \u0623\u0643\u062a\u0648\u0628\u0631",
                    "\u0627\u0644\u0634\u064a\u062e \u0632\u0627\u064a\u062f",
                    "\u0627\u0644\u0639\u0627\u0635\u0645\u0629 \u0627\u0644\u0625\u062f\u0627\u0631\u064a\u0629"
                ]
            },
            new()
            {
                Id = "alexandria",
                Name = "Alexandria",
                NameAr = "\u0627\u0644\u0625\u0633\u0643\u0646\u062f\u0631\u064a\u0629",
                CitiesCovered =
                [
                    "\u0627\u0644\u0625\u0633\u0643\u0646\u062f\u0631\u064a\u0629",
                    "\u0628\u0631\u062c \u0627\u0644\u0639\u0631\u0628",
                    "\u0645\u0637\u0631\u0648\u062d"
                ]
            },
            new()
            {
                Id = "delta",
                Name = "Delta",
                NameAr = "\u0627\u0644\u062f\u0644\u062a\u0627",
                CitiesCovered =
                [
                    "\u0627\u0644\u0645\u0646\u0635\u0648\u0631\u0629",
                    "\u0637\u0646\u0637\u0627",
                    "\u0627\u0644\u0645\u062d\u0644\u0629 \u0627\u0644\u0643\u0628\u0631\u0649",
                    "\u062f\u0645\u0646\u0647\u0648\u0631",
                    "\u0643\u0641\u0631 \u0627\u0644\u0634\u064a\u062e",
                    "\u0627\u0644\u0632\u0642\u0627\u0632\u064a\u0642",
                    "\u0628\u0646\u0647\u0627",
                    "\u0634\u0628\u064a\u0646 \u0627\u0644\u0643\u0648\u0645"
                ]
            },
            new()
            {
                Id = "upper-egypt",
                Name = "Upper Egypt",
                NameAr = "\u0635\u0639\u064a\u062f \u0645\u0635\u0631",
                CitiesCovered =
                [
                    "\u0623\u0633\u064a\u0648\u0637",
                    "\u0633\u0648\u0647\u0627\u062c",
                    "\u0642\u0646\u0627",
                    "\u0627\u0644\u0623\u0642\u0635\u0631",
                    "\u0623\u0633\u0648\u0627\u0646",
                    "\u0627\u0644\u0645\u0646\u064a\u0627",
                    "\u0628\u0646\u064a \u0633\u0648\u064a\u0641",
                    "\u0627\u0644\u0641\u064a\u0648\u0645"
                ]
            },
            new()
            {
                Id = "canal",
                Name = "Canal Cities",
                NameAr = "\u0627\u0644\u0642\u0646\u0627\u0644",
                CitiesCovered =
                [
                    "\u0628\u0648\u0631\u0633\u0639\u064a\u062f",
                    "\u0627\u0644\u0625\u0633\u0645\u0627\u0639\u064a\u0644\u064a\u0629",
                    "\u0627\u0644\u0633\u0648\u064a\u0633"
                ]
            },
            new()
            {
                Id = "sinai",
                Name = "Sinai",
                NameAr = "\u0633\u064a\u0646\u0627\u0621",
                CitiesCovered =
                [
                    "\u0627\u0644\u0639\u0631\u064a\u0634",
                    "\u0634\u0631\u0645 \u0627\u0644\u0634\u064a\u062e",
                    "\u062f\u0647\u0628",
                    "\u0637\u0627\u0628\u0627",
                    "\u0646\u0648\u064a\u0628\u0639"
                ]
            }
        };

        await context.ShippingZones.AddRangeAsync(zones);
        logger.LogInformation("Seeded {Count} shipping zones", zones.Count);
    }

    /// <summary>
    /// Seeds the 8 product categories with their Arabic names, icons, and uplift rates.
    /// Categories mirror the UPLIFT_MATRIX from the TypeScript constants.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    private static async Task SeedProductCategoriesAsync(AroobaDbContext context, ILogger logger)
    {
        if (await context.ProductCategories.AnyAsync())
        {
            logger.LogInformation("Product categories already seeded, skipping");
            return;
        }

        var categories = new List<ProductCategory>
        {
            new()
            {
                Id = "jewelry-accessories",
                NameEn = "Jewelry & Accessories",
                NameAr = "\u0645\u062c\u0648\u0647\u0631\u0627\u062a \u0648\u0625\u0643\u0633\u0633\u0648\u0627\u0631\u0627\u062a",
                Icon = "\ud83d\udc8d",
                MinUpliftRate = 0.15m,
                MaxUpliftRate = 0.18m,
                DefaultUpliftRate = 0.15m,
                Risk = "low"
            },
            new()
            {
                Id = "fashion-apparel",
                NameEn = "Fashion & Apparel",
                NameAr = "\u0623\u0632\u064a\u0627\u0621 \u0648\u0645\u0644\u0627\u0628\u0633",
                Icon = "\ud83d\udc57",
                MinUpliftRate = 0.22m,
                MaxUpliftRate = 0.25m,
                DefaultUpliftRate = 0.22m,
                Risk = "high"
            },
            new()
            {
                Id = "home-decor-fragile",
                NameEn = "Home Decor (Fragile)",
                NameAr = "\u062f\u064a\u0643\u0648\u0631 (\u0647\u0634)",
                Icon = "\ud83c\udffa",
                MinUpliftRate = 0.25m,
                MaxUpliftRate = 0.30m,
                DefaultUpliftRate = 0.25m,
                Risk = "high"
            },
            new()
            {
                Id = "home-decor-textiles",
                NameEn = "Home Decor (Textiles)",
                NameAr = "\u062f\u064a\u0643\u0648\u0631 (\u0645\u0646\u0633\u0648\u062c\u0627\u062a)",
                Icon = "\ud83e\uddf6",
                MinUpliftRate = 0.20m,
                MaxUpliftRate = 0.20m,
                DefaultUpliftRate = 0.20m,
                Risk = "medium"
            },
            new()
            {
                Id = "leather-goods",
                NameEn = "Leather Goods",
                NameAr = "\u0645\u0646\u062a\u062c\u0627\u062a \u062c\u0644\u062f\u064a\u0629",
                Icon = "\ud83d\udc5c",
                MinUpliftRate = 0.20m,
                MaxUpliftRate = 0.20m,
                DefaultUpliftRate = 0.20m,
                Risk = "medium"
            },
            new()
            {
                Id = "beauty-personal",
                NameEn = "Beauty & Personal Care",
                NameAr = "\u062c\u0645\u0627\u0644 \u0648\u0639\u0646\u0627\u064a\u0629 \u0634\u062e\u0635\u064a\u0629",
                Icon = "\ud83e\uddf4",
                MinUpliftRate = 0.20m,
                MaxUpliftRate = 0.20m,
                DefaultUpliftRate = 0.20m,
                Risk = "medium"
            },
            new()
            {
                Id = "furniture-woodwork",
                NameEn = "Furniture & Woodwork",
                NameAr = "\u0623\u062b\u0627\u062b \u0648\u0623\u0639\u0645\u0627\u0644 \u062e\u0634\u0628\u064a\u0629",
                Icon = "\ud83e\ude91",
                MinUpliftRate = 0.15m,
                MaxUpliftRate = 0.15m,
                DefaultUpliftRate = 0.15m,
                Risk = "medium"
            },
            new()
            {
                Id = "food-essentials",
                NameEn = "Food & Essentials",
                NameAr = "\u0623\u063a\u0630\u064a\u0629 \u0648\u0645\u0633\u062a\u0644\u0632\u0645\u0627\u062a",
                Icon = "\ud83c\udf5a",
                MinUpliftRate = 0.10m,
                MaxUpliftRate = 0.15m,
                DefaultUpliftRate = 0.12m,
                Risk = "low"
            }
        };

        await context.ProductCategories.AddRangeAsync(categories);
        logger.LogInformation("Seeded {Count} product categories", categories.Count);
    }

    /// <summary>
    /// Seeds sample rate cards between the shipping zones.
    /// Rates increase with distance: intra-zone is cheapest, cross-country is most expensive.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    private static async Task SeedRateCardsAsync(AroobaDbContext context, ILogger logger)
    {
        if (await context.RateCards.AnyAsync())
        {
            logger.LogInformation("Rate cards already seeded, skipping");
            return;
        }

        var zoneIds = new[] { "cairo", "alexandria", "delta", "upper-egypt", "canal", "sinai" };

        // Base rates matrix (from -> to): base price, per-kg rate
        var rateData = new Dictionary<(string From, string To), (decimal BasePrice, decimal PerKg)>
        {
            // Cairo rates
            [("cairo", "cairo")] = (35m, 10m),
            [("cairo", "alexandria")] = (50m, 15m),
            [("cairo", "delta")] = (45m, 12m),
            [("cairo", "upper-egypt")] = (65m, 20m),
            [("cairo", "canal")] = (50m, 15m),
            [("cairo", "sinai")] = (80m, 25m),

            // Alexandria rates
            [("alexandria", "alexandria")] = (35m, 10m),
            [("alexandria", "cairo")] = (50m, 15m),
            [("alexandria", "delta")] = (45m, 12m),
            [("alexandria", "upper-egypt")] = (70m, 22m),
            [("alexandria", "canal")] = (55m, 18m),
            [("alexandria", "sinai")] = (85m, 25m),

            // Delta rates
            [("delta", "delta")] = (35m, 10m),
            [("delta", "cairo")] = (45m, 12m),
            [("delta", "alexandria")] = (45m, 12m),
            [("delta", "upper-egypt")] = (65m, 20m),
            [("delta", "canal")] = (45m, 12m),
            [("delta", "sinai")] = (80m, 25m),

            // Upper Egypt rates
            [("upper-egypt", "upper-egypt")] = (40m, 12m),
            [("upper-egypt", "cairo")] = (65m, 20m),
            [("upper-egypt", "alexandria")] = (70m, 22m),
            [("upper-egypt", "delta")] = (65m, 20m),
            [("upper-egypt", "canal")] = (65m, 20m),
            [("upper-egypt", "sinai")] = (90m, 28m),

            // Canal rates
            [("canal", "canal")] = (35m, 10m),
            [("canal", "cairo")] = (50m, 15m),
            [("canal", "alexandria")] = (55m, 18m),
            [("canal", "delta")] = (45m, 12m),
            [("canal", "upper-egypt")] = (65m, 20m),
            [("canal", "sinai")] = (60m, 18m),

            // Sinai rates
            [("sinai", "sinai")] = (45m, 15m),
            [("sinai", "cairo")] = (80m, 25m),
            [("sinai", "alexandria")] = (85m, 25m),
            [("sinai", "delta")] = (80m, 25m),
            [("sinai", "upper-egypt")] = (90m, 28m),
            [("sinai", "canal")] = (60m, 18m),
        };

        var rateCards = rateData.Select(kvp => new RateCard
        {
            Id = Guid.NewGuid(),
            FromZoneId = kvp.Key.From,
            ToZoneId = kvp.Key.To,
            BasePrice = kvp.Value.BasePrice,
            PricePerKg = kvp.Value.PerKg,
        }).ToList();

        await context.RateCards.AddRangeAsync(rateCards);
        logger.LogInformation("Seeded {Count} rate cards", rateCards.Count);
    }

    /// <summary>
    /// Seeds a super admin user account for initial platform access.
    /// Uses a well-known GUID so this seed is idempotent.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    private static async Task SeedSuperAdminAsync(AroobaDbContext context, ILogger logger)
    {
        var superAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        if (await context.Users.AnyAsync(u => u.Id == superAdminId))
        {
            logger.LogInformation("Super admin user already seeded, skipping");
            return;
        }

        var superAdmin = new User
        {
            Id = superAdminId,
            MobileNumber = "+201000000001",
            Email = "admin@aroobh.com",
            FullName = "Arooba Super Admin",
            FullNameAr = "\u0645\u0633\u0624\u0648\u0644 \u0623\u0631\u0648\u0628\u0629 \u0627\u0644\u0631\u0626\u064a\u0633\u064a",
            Role = UserRole.AdminSuper,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            LastModifiedBy = "system",
        };

        await context.Users.AddAsync(superAdmin);
        logger.LogInformation("Seeded super admin user with ID {Id}", superAdminId);
    }
}
