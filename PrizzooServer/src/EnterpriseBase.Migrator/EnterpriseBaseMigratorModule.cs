using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using EnterpriseBase.Configuration;
using EnterpriseBase.EntityFrameworkCore;
using EnterpriseBase.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace EnterpriseBase.Migrator;

[DependsOn(typeof(EnterpriseBaseEntityFrameworkModule))]
public class EnterpriseBaseMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public EnterpriseBaseMigratorModule(EnterpriseBaseEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(EnterpriseBaseMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            EnterpriseBaseConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(EnterpriseBaseMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
