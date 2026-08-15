using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using EnterpriseBase.EntityFrameworkCore.Seed.Host;
using EnterpriseBase.EntityFrameworkCore.Seed.Tenants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Transactions;

namespace EnterpriseBase.EntityFrameworkCore.Seed;

public static class SeedHelper
{
    public static void SeedHostDb(IIocResolver iocResolver)
    {
        WithDbContext<EnterpriseBaseDbContext>(iocResolver, SeedHostDb);
    }

    public static void SeedHostDb(EnterpriseBaseDbContext context)
    {
        context.SuppressAutoSetTenantId = true;

        // Host seed
        new InitialHostDbBuilder(context).Create();

        // Default tenant seed — TenantRoleAndUserBuilder handles branch, accounts, employee types, financial year
        new DefaultTenantBuilder(context).Create();

        var tenantIds = context.Tenants
            .Where(t => !t.IsDeleted)
            .Select(t => t.Id)
            .ToList();

        foreach (var tenantId in tenantIds)
        {
            new TenantRoleAndUserBuilder(context, tenantId).Create();
        }
    }

    private static void WithDbContext<TDbContext>(IIocResolver iocResolver, Action<TDbContext> contextAction)
        where TDbContext : DbContext
    {
        using (var uowManager = iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
        {
            using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
            {
                var context = uowManager.Object.Current.GetDbContext<TDbContext>(MultiTenancySides.Host);

                contextAction(context);

                uow.Complete();
            }
        }
    }
}
