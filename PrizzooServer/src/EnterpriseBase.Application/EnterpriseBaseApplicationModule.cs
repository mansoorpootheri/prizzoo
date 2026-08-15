using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using EnterpriseBase.Authorization;
using QuestPDF.Infrastructure;

namespace EnterpriseBase;

[DependsOn(
    typeof(EnterpriseBaseCoreModule),
    typeof(AbpAutoMapperModule))]
public class EnterpriseBaseApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        // Configure QuestPDF to use Community License
        QuestPDF.Settings.License = LicenseType.Community;
        
        Configuration.Authorization.Providers.Add<EnterpriseBaseAuthorizationProvider>();


        //Adding custom AutoMapper configuration
        Configuration.Modules.AbpAutoMapper().Configurators.Add(CustomDtoMapper.CreateMappings);
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(EnterpriseBaseApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
