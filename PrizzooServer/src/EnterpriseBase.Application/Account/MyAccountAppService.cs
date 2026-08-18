using Abp.Application.Services;
using Abp.Authorization;
using Abp.UI;
using EnterpriseBase.Users.Dto;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Account
{
    public interface IMyAccountAppService : IApplicationService
    {
        /// <summary>Any authenticated user (Admin, ShopOwner, Shopper) changes their own password.</summary>
        Task ChangeMyPasswordAsync(ChangePasswordDto input);
    }

    /// <summary>
    /// Self-service account actions for the currently logged-in user.
    /// Deliberately separate from UserAppService.ChangePassword, which is
    /// gated by Pages_Users (a host/tenant user-management permission) and
    /// would 403 for ShopOwner/Shopper trying to change their own password -
    /// this endpoint only requires being logged in, and always operates on
    /// AbpSession's own user, never an arbitrary target.
    /// </summary>
    [AbpAuthorize]
    public class MyAccountAppService : EnterpriseBaseAppServiceBase, IMyAccountAppService
    {
        public virtual async Task ChangeMyPasswordAsync(ChangePasswordDto input)
        {
            var user = await GetCurrentUserAsync();

            if (!await UserManager.CheckPasswordAsync(user, input.CurrentPassword))
                throw new UserFriendlyException("Current password is incorrect.");

            CheckErrors(await UserManager.ChangePasswordAsync(user, input.NewPassword));
        }
    }
}
