using Abp.Localization;
using Abp.Runtime.Session;
using Abp.Zero.EntityFrameworkCore;
using EnterpriseBase.Authorization.Delegation;
using EnterpriseBase.Authorization.Otp;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.ExtraProperties;
using EnterpriseBase.Geography;
using EnterpriseBase.Editions;
using EnterpriseBase.MasterData;
using EnterpriseBase.MultiTenancy;
using EnterpriseBase.Pricing;
using EnterpriseBase.ReleaseNotes;
using EnterpriseBase.Storage;
using EnterpriseBase.Stores;
using EnterpriseBase.Taxes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EnterpriseBase.EntityFrameworkCore;

public class EnterpriseBaseDbContext : AbpZeroDbContext<Tenant, Role, User, EnterpriseBaseDbContext>
{
    /* Define a DbSet for each entity of the application */
    public virtual DbSet<BinaryObject> BinaryObjects { get; set; }
    public virtual DbSet<UserDelegation> UserDelegations { get; set; }
    public virtual DbSet<OtpChallenge> OtpChallenges { get; set; }
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<State> States { get; set; }
    public virtual DbSet<District> Districts { get; set; }
    public virtual DbSet<Tax> Taxes { get; set; }
    public virtual DbSet<SubscriptionRequest> SubscriptionRequests { get; set; }
    public virtual DbSet<ReleaseNote> ReleaseNotes { get; set; }

    // Prizzoo public catalog - shared, not tenant-scoped
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Unit> Units { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<StoreChain> StoreChains { get; set; }
    public virtual DbSet<Store> Stores { get; set; }
    public virtual DbSet<Price> Prices { get; set; }
    public virtual DbSet<ProductRating> ProductRatings { get; set; }
    public virtual DbSet<Location> Locations { get; set; }
    public virtual DbSet<Flyer> Flyers { get; set; }
    public virtual DbSet<FlyerProduct> FlyerProducts { get; set; }

    public EnterpriseBaseDbContext(DbContextOptions<EnterpriseBaseDbContext> options)
        : base(options)
    {
    }
    // add these lines to override max length of property
    // we should set max length smaller than the PostgreSQL allowed size (10485760)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EnterpriseEdition>(b =>
        {
            b.HasBaseType<Abp.Application.Editions.Edition>();
            b.Property(e => e.MonthlyPrice).HasColumnType("numeric(18,2)");
            b.Property(e => e.AnnualPrice).HasColumnType("numeric(18,2)");
            b.Property(e => e.GstRate).HasColumnType("numeric(5,2)").HasDefaultValue(18m);
            b.Property(e => e.HsnSacCode).HasMaxLength(20).HasDefaultValue("998314");
            b.Ignore(e => e.IsFree);
            b.Ignore(e => e.MonthlyPriceExclGst);
            b.Ignore(e => e.MonthlyGstAmount);
            b.Ignore(e => e.AnnualPriceExclGst);
            b.Ignore(e => e.AnnualGstAmount);
        });

        modelBuilder.Entity<ApplicationLanguageText>()
            .Property(p => p.Value)
            .HasMaxLength(100); // any integer that is smaller than 10485760

        // Prices are the core comparison data - never cascade-delete just
        // because a Product or Store gets removed; keep as append-only history.
        modelBuilder.Entity<Price>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Price>()
            .HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Price>()
            .HasIndex(x => new { x.ProductId, x.StoreId, x.Status });

        modelBuilder.Entity<Store>()
            .HasOne(x => x.Chain)
            .WithMany()
            .HasForeignKey(x => x.ChainId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Store>()
            .HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Location>()
            .HasOne(x => x.District)
            .WithMany()
            .HasForeignKey(x => x.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Store>()
            .HasIndex(x => new { x.Latitude, x.Longitude });

        modelBuilder.Entity<ProductRating>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // One rating per shopper per product - rating again overwrites,
        // enforced at the DB level, not just in application code.
        modelBuilder.Entity<ProductRating>()
            .HasIndex(x => new { x.ProductId, x.ShopperUserId })
            .IsUnique();

        modelBuilder.Entity<Flyer>()
            .HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Flyer>()
            .HasOne(x => x.Image)
            .WithMany()
            .HasForeignKey(x => x.ImageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Flyer>()
            .HasIndex(x => x.StoreId);

        modelBuilder.Entity<FlyerProduct>()
            .HasOne(x => x.Flyer)
            .WithMany()
            .HasForeignKey(x => x.FlyerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FlyerProduct>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // A product shouldn't be linked to the same flyer twice.
        modelBuilder.Entity<FlyerProduct>()
            .HasIndex(x => new { x.FlyerId, x.ProductId })
            .IsUnique();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp");
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified)));
                }
            }
        }
    }

}
