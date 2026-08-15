using EnterpriseBase.MasterData;
using EnterpriseBase.Pricing;
using EnterpriseBase.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EnterpriseBase.EntityFrameworkCore.Seed.Host;

/// <summary>
/// MVP launch-category demo data for Kozhikode (the Tier-2 city named in the
/// business plan and used throughout the Figma design's location header):
/// a handful of electronics/mobile stores, products, and prices so the
/// shopper-facing search actually has something to return. See
/// DOCS/PRIZZOO_MIGRATION_NOTES.md "Known gaps" #6.
///
/// Store names are fictional to avoid attributing fabricated pricing to real
/// businesses; product names are real device models, which is standard for
/// any price-comparison catalog.
/// </summary>
public class DefaultCatalogCreator
{
    private readonly EnterpriseBaseDbContext _context;

    public DefaultCatalogCreator(EnterpriseBaseDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        var categoryExists = _context.Categories.IgnoreQueryFilters().Any(c => c.Code == "ELEC");
        if (categoryExists) return;

        var category = new Category
        {
            Name = "Electronics & Mobiles",
            Code = "ELEC",
            Description = "Phones, audio, and accessories"
        };
        _context.Categories.Add(category);

        var unit = new Unit
        {
            Name = "Piece",
            Code = "PCS",
            DecimalPlaces = 0
        };
        _context.Units.Add(unit);

        var gadgetHub = new Store
        {
            Name = "GadgetHub Kozhikode",
            Address = "Mavoor Road",
            City = "Kozhikode",
            Latitude = 11.2620m,
            Longitude = 75.7920m,
            IsVerified = true
        };
        var digitalBazaar = new Store
        {
            Name = "Digital Bazaar",
            Address = "Hilite Mall",
            City = "Kozhikode",
            Latitude = 11.2735m,
            Longitude = 75.7860m,
            IsVerified = true
        };
        var techZone = new Store
        {
            Name = "TechZone Calicut",
            Address = "SM Street",
            City = "Kozhikode",
            Latitude = 11.2510m,
            Longitude = 75.7770m,
            IsVerified = true
        };
        var electroMart = new Store
        {
            Name = "ElectroMart",
            Address = "Palazhi",
            City = "Kozhikode",
            Latitude = 11.2830m,
            Longitude = 75.7690m,
            IsVerified = false
        };
        var mobilePoint = new Store
        {
            Name = "Mobile Point",
            Address = "Beach Road",
            City = "Kozhikode",
            Latitude = 11.2480m,
            Longitude = 75.7700m,
            IsVerified = true
        };
        _context.Stores.AddRange(gadgetHub, digitalBazaar, techZone, electroMart, mobilePoint);

        Product MakeProduct(string name) => new()
        {
            Name = name,
            Category = category,
            Unit = unit
        };

        var galaxyM14 = MakeProduct("Samsung Galaxy M14 5G");
        var redmiNote13 = MakeProduct("Redmi Note 13");
        var realmeNarzo60 = MakeProduct("Realme Narzo 60");
        var vivoY28 = MakeProduct("Vivo Y28");
        var oneplusNordCe4 = MakeProduct("OnePlus Nord CE 4");
        var boatAirdopes141 = MakeProduct("boAt Airdopes 141");
        var jblGo3 = MakeProduct("JBL Go 3 Speaker");
        var miPowerBank = MakeProduct("Mi Power Bank 10000mAh");
        _context.Products.AddRange(
            galaxyM14, redmiNote13, realmeNarzo60, vivoY28,
            oneplusNordCe4, boatAirdopes141, jblGo3, miPowerBank);

        var now = DateTime.UtcNow;

        Price MakePrice(Product product, Store store, decimal amount, DateTime observedAt) => new()
        {
            Product = product,
            Store = store,
            Amount = amount,
            Source = PriceSource.RetailerReported,
            Status = PriceStatus.Approved,
            ObservedAt = observedAt
        };

        _context.Prices.AddRange(
            MakePrice(galaxyM14, techZone, 13799m, now.AddHours(-5)),
            MakePrice(galaxyM14, gadgetHub, 13999m, now.AddHours(-8)),
            MakePrice(galaxyM14, digitalBazaar, 14499m, now.AddDays(-1)),
            MakePrice(galaxyM14, mobilePoint, 14999m, now.AddDays(-10)), // deliberately stale

            MakePrice(redmiNote13, gadgetHub, 16799m, now.AddHours(-3)),
            MakePrice(redmiNote13, digitalBazaar, 16999m, now.AddHours(-12)),
            MakePrice(redmiNote13, electroMart, 17499m, now.AddDays(-2)),

            MakePrice(realmeNarzo60, techZone, 14999m, now.AddHours(-6)),
            MakePrice(realmeNarzo60, mobilePoint, 15299m, now.AddDays(-1)),

            MakePrice(vivoY28, gadgetHub, 11999m, now.AddHours(-4)),
            MakePrice(vivoY28, digitalBazaar, 12299m, now.AddDays(-3)),

            MakePrice(oneplusNordCe4, techZone, 24499m, now.AddHours(-9)),
            MakePrice(oneplusNordCe4, electroMart, 24999m, now.AddDays(-2)),

            MakePrice(boatAirdopes141, digitalBazaar, 1199m, now.AddHours(-2)),
            MakePrice(boatAirdopes141, mobilePoint, 1299m, now.AddHours(-7)),
            MakePrice(boatAirdopes141, gadgetHub, 1349m, now.AddDays(-4)),

            MakePrice(jblGo3, techZone, 2799m, now.AddHours(-10)),
            MakePrice(jblGo3, electroMart, 2999m, now.AddDays(-1)),

            MakePrice(miPowerBank, gadgetHub, 899m, now.AddHours(-1)),
            MakePrice(miPowerBank, mobilePoint, 949m, now.AddDays(-5))
        );

        _context.SaveChanges();
    }
}
