using Abp.Application.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Branches;
using EnterpriseBase.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace EnterpriseBase;

public abstract class EnterpriseBaseAppServiceBase : ApplicationService
{
    public TenantManager TenantManager { get; set; }
    public UserManager UserManager { get; set; }
    public BranchManager BranchManager { get; set; }

    protected EnterpriseBaseAppServiceBase()
    {
        LocalizationSourceName = EnterpriseBaseConsts.LocalizationSourceName;
    }

    protected virtual int GetCurrentBranchId() => BranchManager.GetCurrentBranchId();

    protected virtual async Task<User> GetCurrentUserAsync()
    {
        var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
        if (user == null)
        {
            throw new Exception("There is no current user!");
        }

        return user;
    }

    protected virtual Task<Tenant> GetCurrentTenantAsync()
    {
        return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
    }

    protected virtual void CheckErrors(IdentityResult identityResult)
    {
        identityResult.CheckErrors(LocalizationManager);
    }
}
