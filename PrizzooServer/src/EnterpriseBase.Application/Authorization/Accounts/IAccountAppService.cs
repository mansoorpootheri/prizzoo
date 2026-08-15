using Abp.Application.Services;
using EnterpriseBase.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace EnterpriseBase.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);

    Task<TenantSignupOutput> TenantSignup(TenantSignupInput input);

    Task<ImpersonateOutput> ImpersonateUser(ImpersonateUserInput input);

    Task<ImpersonateOutput> ImpersonateTenant(ImpersonateTenantInput input);

    Task<ImpersonateOutput> DelegatedImpersonate(DelegatedImpersonateInput input);

    Task<ImpersonateOutput> BackToImpersonator();
}
