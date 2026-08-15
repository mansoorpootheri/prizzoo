using Abp.Authorization;
using Abp.Runtime.Session;
using EnterpriseBase.Configuration.Dto;
using System.Threading.Tasks;

namespace EnterpriseBase.Configuration;

[AbpAuthorize]
public class ConfigurationAppService : EnterpriseBaseAppServiceBase, IConfigurationAppService
{
    public async Task ChangeUiTheme(ChangeUiThemeInput input)
    {
        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
    }
}
