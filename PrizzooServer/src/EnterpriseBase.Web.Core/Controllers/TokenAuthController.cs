using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.UI;
using EnterpriseBase.Authentication.JwtBearer;
using EnterpriseBase.Authorization;
using EnterpriseBase.Authorization.Impersonation;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Models.TokenAuth;
using EnterpriseBase.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EnterpriseBase.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : JwtIssuingControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly UserManager _userManager;
        private readonly IImpersonationManager _impersonationManager;

        protected override TokenAuthConfiguration Configuration => _configuration;

        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            UserManager userManager,
            IImpersonationManager impersonationManager)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _userManager = userManager;
            _impersonationManager = impersonationManager;
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                await GetTenancyNameOrNull(model.UserNameOrEmailAddress)
            );

            if (loginResult?.Tenant?.Id != AbpSession.TenantId)
            {
                SetTenantIdCookie(loginResult?.Tenant?.Id);
                CurrentUnitOfWork.SetTenantId(loginResult?.Tenant?.Id);
            }

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }

        [HttpPost]
        [AbpAuthorize]
        [UnitOfWork]
        public async Task<AuthenticateResultModel> ImpersonateAuthenticate([FromBody] ImpersonateAuthenticateModel model)
        {
            var result = await _impersonationManager.GetImpersonatedUserAndIdentity(model.ImpersonationToken);

            var accessToken = CreateAccessToken(CreateJwtClaims(result.Identity));

            // Set tenant cookie so subsequent requests resolve to the impersonated tenant
            SetTenantIdCookie(result.User.TenantId);

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = result.User.Id
            };
        }

        private async Task<string> GetTenancyNameOrNull(string email)
        {
            var tenantId = await _userManager.TryGetTenantIdOfUser(email);
            if (!tenantId.HasValue)
            {
                return null;
            }
            return _tenantCache.GetOrNull(tenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

    }
}
