using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with reference data required for the Arooba Marketplace to function.
/// Includes shipping zones, product categories, and an admin user.
/// </summary>
public static class AroobaDbContextSeed
{
    /// <summary>Seeds reference data into the database if not already present.</summary>
    public static async Task SeedAsync(AroobaDbContext context)
    {
        // ── Shipping Zones (Egypt) ──
        if (!await context.ShippingZones.AnyAsync())
        {
            context.ShippingZones.AddRange(
                new ShippingZone
                {
                    Id = "cairo", Name = "Greater Cairo", NameAr = "القاهرة الكبرى",
                    CitiesCovered = new List<string> { "Cairo", "Giza", "6th October", "New Cairo", "Helwan", "Shoubra" }
                },
                new ShippingZone
                {
                    Id = "alexandria", Name = "Alexandria", NameAr = "الإسكندرية",
                    CitiesCovered = new List<string> { "Alexandria", "Borg El Arab", "Montaza" }
                },
                new ShippingZone
                {
                    Id = "delta", Name = "Delta", NameAr = "الدلتا",
                    CitiesCovered = new List<string> { "Tanta", "Mansoura", "Zagazig", "Damanhur", "Kafr El Sheikh" }
                },
                new ShippingZone
                {
                    Id = "upper-egypt", Name = "Upper Egypt", NameAr = "صعيد مصر",
                    CitiesCovered = new List<string> { "Assiut", "Sohag", "Qena", "Luxor", "Aswan", "Minya" }
                },
                new ShippingZone
                {
                    Id = "canal", Name = "Canal Cities", NameAr = "القنال",
                    CitiesCovered = new List<string> { "Port Said", "Suez", "Ismailia" }
                },
                new ShippingZone
                {
                    Id = "sinai", Name = "Sinai", NameAr = "سيناء",
                    CitiesCovered = new List<string> { "Sharm El Sheikh", "El Arish", "Dahab", "Nuweiba" }
                });
        }

        // ── Product Categories with Uplift Rates ──
        if (!await context.ProductCategories.AnyAsync())
        {
            context.ProductCategories.AddRange(
                new ProductCategory { Id = "jewelry-accessories", NameAr = "مجوهرات وإكسسوارات", NameEn = "Jewelry & Accessories", Icon = "ring", MinUplift = 0.15m, MaxUplift = 0.18m, DefaultUplift = 0.15m, RiskLevel = "low" },
                new ProductCategory { Id = "fashion-apparel", NameAr = "أزياء وملابس", NameEn = "Fashion & Apparel", Icon = "shirt", MinUplift = 0.22m, MaxUplift = 0.25m, DefaultUplift = 0.22m, RiskLevel = "high" },
                new ProductCategory { Id = "home-decor-fragile", NameAr = "ديكور (هش)", NameEn = "Home Decor (Fragile)", Icon = "vase", MinUplift = 0.25m, MaxUplift = 0.30m, DefaultUplift = 0.25m, RiskLevel = "high" },
                new ProductCategory { Id = "home-decor-textiles", NameAr = "ديكور (منسوجات)", NameEn = "Home Decor (Textiles)", Icon = "thread", MinUplift = 0.20m, MaxUplift = 0.20m, DefaultUplift = 0.20m, RiskLevel = "medium" },
                new ProductCategory { Id = "leather-goods", NameAr = "منتجات جلدية", NameEn = "Leather Goods", Icon = "bag", MinUplift = 0.20m, MaxUplift = 0.20m, DefaultUplift = 0.20m, RiskLevel = "medium" },
                new ProductCategory { Id = "beauty-personal", NameAr = "جمال وعناية شخصية", NameEn = "Beauty & Personal Care", Icon = "sparkles", MinUplift = 0.20m, MaxUplift = 0.20m, DefaultUplift = 0.20m, RiskLevel = "medium" },
                new ProductCategory { Id = "furniture-woodwork", NameAr = "أثاث وأعمال خشبية", NameEn = "Furniture & Woodwork", Icon = "armchair", MinUplift = 0.15m, MaxUplift = 0.15m, DefaultUplift = 0.15m, RiskLevel = "medium" },
                new ProductCategory { Id = "food-essentials", NameAr = "أغذية ومستلزمات", NameEn = "Food & Essentials", Icon = "wheat", MinUplift = 0.10m, MaxUplift = 0.15m, DefaultUplift = 0.12m, RiskLevel = "low" });
        }

        // ── Rate Cards (inter-zone shipping rates) ──
        if (!await context.RateCards.AnyAsync())
        {
            var zones = new[] { "cairo", "alexandria", "delta", "upper-egypt", "canal", "sinai" };
            var rates = new Dictionary<string, (decimal basePrice, decimal perKg)>
            {
                ["cairo-cairo"] = (35, 5),
                ["cairo-alexandria"] = (45, 7),
                ["cairo-delta"] = (45, 7),
                ["cairo-upper-egypt"] = (55, 10),
                ["cairo-canal"] = (45, 7),
                ["cairo-sinai"] = (65, 12),
                ["alexandria-alexandria"] = (35, 5),
                ["alexandria-delta"] = (40, 6),
                ["delta-delta"] = (35, 5),
                ["upper-egypt-upper-egypt"] = (40, 6),
                ["canal-canal"] = (35, 5),
                ["sinai-sinai"] = (40, 6),
            };

            foreach (var (key, (basePrice, perKg)) in rates)
            {
                var parts = key.Split('-', 2);
                // Handle composite keys like "upper-egypt-upper-egypt"
                var fromZone = parts[0];
                var toZone = parts.Length > 1 ? parts[1] : parts[0];

                // Fix for zones with hyphens
                if (key.StartsWith("upper-egypt"))
                {
                    fromZone = "upper-egypt";
                    toZone = key.Replace("upper-egypt-", "");
                    if (toZone == "upper") toZone = "upper-egypt";
                }

                context.RateCards.Add(new RateCard
                {
                    Id = Guid.NewGuid(),
                    FromZoneId = fromZone,
                    ToZoneId = toZone,
                    BasePrice = basePrice,
                    PricePerKg = perKg
                });
            }
        }

        // ── Admin User ──
        if (!await context.Users.AnyAsync(u => u.Role == UserRole.AdminSuper))
        {
            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                MobileNumber = "+201000000000",
                Email = "admin@aroobh.com",
                FullName = "Arooba Admin",
                FullNameAr = "مدير أروبة",
                Role = UserRole.AdminSuper,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }
}
