using Abp.Auditing;
using EnterpriseBase.Sessions.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Sessions;

public class SessionAppService : EnterpriseBaseAppServiceBase, ISessionAppService
{
    [DisableAuditing]
    public async Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations()
    {
        var output = new GetCurrentLoginInformationsOutput
        {
            Application = new ApplicationInfoDto
            {
                Version = AppVersionHelper.Version,
                ReleaseDate = AppVersionHelper.ReleaseDate,
                Features = new Dictionary<string, bool>()
            }
        };

        try
        {
            if (AbpSession.TenantId.HasValue)
            {
                output.Tenant = ObjectMapper.Map<TenantLoginInfoDto>(await GetCurrentTenantAsync());
            }

            if (AbpSession.UserId.HasValue)
            {
                output.User = ObjectMapper.Map<UserLoginInfoDto>(await GetCurrentUserAsync());
            }
        }
        catch (System.OperationCanceledException)
        {
            // Handle cancellation gracefully
        }

        return output;
    }
}
