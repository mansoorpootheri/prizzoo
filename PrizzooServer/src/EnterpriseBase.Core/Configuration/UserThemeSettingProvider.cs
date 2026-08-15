using System.Collections.Generic;
using Abp.Configuration;

namespace EnterpriseBase.Configuration
{
    public class UserThemeSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new[]
            {
                new SettingDefinition("UserTheme", "light", scopes: SettingScopes.User),
                new SettingDefinition(
                    AppSettings.DashboardCustomization.QuickActions,
                    "{\"hidden\":[],\"custom\":[]}",
                    scopes: SettingScopes.User
                )
            };
        }
    }
}