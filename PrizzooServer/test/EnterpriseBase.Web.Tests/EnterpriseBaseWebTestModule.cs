using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using EnterpriseBase.EntityFrameworkCore;
using EnterpriseBase.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace EnterpriseBase.Web.Tests;

[DependsOn(
    typeof(EnterpriseBaseWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class EnterpriseBaseWebTestModule : AbpModule
{
    public EnterpriseBaseWebTestModule(EnterpriseBaseEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(EnterpriseBaseWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(EnterpriseBaseWebMvcModule).Assembly);
    }
}