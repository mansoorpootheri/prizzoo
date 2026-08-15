using EnterpriseBase.Configuration.Dto;
using System.Threading.Tasks;

namespace EnterpriseBase.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
