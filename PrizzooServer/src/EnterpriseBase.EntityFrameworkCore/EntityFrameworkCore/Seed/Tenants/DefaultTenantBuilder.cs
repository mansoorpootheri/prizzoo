using Abp.MultiTenancy;
using EnterpriseBase.Editions;
using EnterpriseBase.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EnterpriseBase.EntityFrameworkCore.Seed.Tenants;

public class DefaultTenantBuilder
{
    private readonly EnterpriseBaseDbContext _context;

    public DefaultTenantBuilder(EnterpriseBaseDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        CreateDefaultTenant();
    }

    private void CreateDefaultTenant()
    {
        var defaultTenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.TenancyName == AbpTenantBase.DefaultTenantName);
        if (defaultTenant == null)
        {
            defaultTenant = new Tenant(AbpTenantBase.DefaultTenantName, AbpTenantBase.DefaultTenantName);
            _context.Tenants.Add(defaultTenant);
            _context.SaveChanges();
        }

        // Always ensure edition + subscription fields are set (free plan = unlimited)
        var freeEdition = _context.Editions.IgnoreQueryFilters().OfType<EnterpriseEdition>().FirstOrDefault(e => e.Name == "Free");
        if (freeEdition != null && defaultTenant.EditionId != freeEdition.Id)
        {
            defaultTenant.EditionId = freeEdition.Id;
            defaultTenant.BillingCycle = null;              // free plan has no billing cycle
            defaultTenant.LastAmountPaid = 0;
            defaultTenant.LastBillingDateUtc = System.DateTime.UtcNow;
            defaultTenant.SubscriptionEndDateUtc = null;    // null = unlimited
            defaultTenant.IsInTrialPeriod = false;
            _context.SaveChanges();
        }
    }
}
