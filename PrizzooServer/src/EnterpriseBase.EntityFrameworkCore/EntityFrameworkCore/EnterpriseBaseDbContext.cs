using Abp.Localization;
using Abp.Runtime.Session;
using Abp.Zero.EntityFrameworkCore;
using EnterpriseBase.Authorization.Delegation;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Branches;
using EnterpriseBase.DataFilters;
using EnterpriseBase.Employees;
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
    public virtual DbSet<UserBranchMapping> UserBranchMappings { get; set; }
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<State> States { get; set; }
    public virtual DbSet<District> Districts { get; set; }
    public virtual DbSet<Branch> Branches { get; set; }
    public virtual DbSet<EmployeeType> EmployeeTypes { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
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

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter("\"UserId\" IS NOT NULL");

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
            .HasIndex(x => new { x.Latitude, x.Longitude });

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

    public IPrincipalAccessor PrincipalAccessor { get; set; }

    protected virtual List<int> CurrentBranchIds => GetCurrentBranchIdOrNull();
    protected virtual bool IsBranchFilterEnabled => CurrentUnitOfWorkProvider?.Current?.IsFilterEnabled("MustHaveBranch") == true;
    protected virtual List<int> GetCurrentBranchIdOrNull()
    {
        var branchClaim = PrincipalAccessor.Principal?.Claims.FirstOrDefault(c => c.Type == "BranchIds");
        if (string.IsNullOrEmpty(branchClaim?.Value))
        {
            return new List<int>();
        }

        var val = branchClaim.Value.Split(",");
        List<int> ids = new List<int>();
        foreach (var item in val)
        {
            ids.Add(Convert.ToInt32(item));
        }
        return ids;
    }

    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        if (typeof(IMustHaveBranch).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }
        return base.ShouldFilterEntity<TEntity>(entityType);
    }
    protected override Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>(ModelBuilder modelBuilder)
    {
        var expression = base.CreateFilterExpression<TEntity>(modelBuilder);
        if (typeof(IMustHaveBranch).IsAssignableFrom(typeof(TEntity)))
        {
            Expression<Func<TEntity, bool>> MayHaveBranch =
                    e => !IsBranchFilterEnabled || CurrentBranchIds.Contains((int)((IMustHaveBranch)e).BranchId);
            expression = expression == null ? MayHaveBranch : CombineExpressions(expression, MayHaveBranch);

        }
        return expression;
    }
}
