using Abp.Localization;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Runtime.Security;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using EnterpriseBase.Authorization.Delegation;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Configuration;
using EnterpriseBase.Features;
using EnterpriseBase.Localization;
using EnterpriseBase.MultiTenancy;
using EnterpriseBase.Timing;

namespace EnterpriseBase;

[DependsOn(typeof(AbpZeroCoreModule))]
public class EnterpriseBaseCoreModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Auditing.IsEnabledForAnonymousUsers = true;

        // Declare entity types
        Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
        Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
        Configuration.Modules.Zero().EntityTypes.User = typeof(User);

        EnterpriseBaseLocalizationConfigurer.Configure(Configuration.Localization);

        //Adding feature providers
        Configuration.Features.Providers.Add<AppFeatureProvider>();

        //Adding setting providers
        Configuration.Settings.Providers.Add<AppSettingProvider>();
        Configuration.Settings.Providers.Add<UserThemeSettingProvider>();

        // Enable this line to create a multi-tenant application.
        Configuration.MultiTenancy.IsEnabled = EnterpriseBaseConsts.MultiTenancyEnabled;

        // Configure roles
        AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

        Configuration.Localization.Languages.Add(new LanguageInfo("fa", "فارسی", "famfamfam-flags ir"));

        Configuration.Settings.SettingEncryptionConfiguration.DefaultPassPhrase = EnterpriseBaseConsts.DefaultPassPhrase;
        SimpleStringCipher.DefaultPassPhrase = EnterpriseBaseConsts.DefaultPassPhrase;
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(EnterpriseBaseCoreModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Register<IUserDelegationConfiguration, UserDelegationConfiguration>();
        IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
    }
}
