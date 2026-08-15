using Abp.Modules;
using Abp.Reflection.Extensions;
using EnterpriseBase.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace EnterpriseBase.Web.Host.Startup
{
    [DependsOn(
       typeof(EnterpriseBaseWebCoreModule))]
    public class EnterpriseBaseWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public EnterpriseBaseWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }
        public override void PreInitialize()
        {
            base.PreInitialize();
            Configuration.Navigation.Providers.Add<EnterpriseBaseNavigationProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(EnterpriseBaseWebHostModule).GetAssembly());
        }
    }
}
