using Abp.Authorization.Users;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using EnterpriseBase.Application.Otp;
using EnterpriseBase.Authentication.JwtBearer;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Models.Otp;
using EnterpriseBase.Models.TokenAuth;
using EnterpriseBase.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EnterpriseBase.Controllers
{
    /// <summary>
    /// Phone+OTP login for shoppers - the only way a shopper account is
    /// created or authenticated now (no password, no self-registration
    /// form). Mirrors TokenAuthController's token-issuance shape exactly
    /// (same AuthenticateResultModel, same JWT signing config) via
    /// JwtIssuingControllerBase, so the resulting token is interchangeable
    /// with a password-issued one everywhere [AbpAuthorize] is used - the
    /// same mechanism ImpersonationManager already relies on to build a
    /// ClaimsIdentity from an already-resolved user without a password.
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class OtpAuthController : JwtIssuingControllerBase
    {
        private readonly IOtpChallengeService _otpChallengeService;
        private readonly UserManager _userManager;
        private readonly UserClaimsPrincipalFactory _principalFactory;
        private readonly TenantManager _tenantManager;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        protected override TokenAuthConfiguration Configuration => _configuration;

        public OtpAuthController(
            IOtpChallengeService otpChallengeService,
            UserManager userManager,
            UserClaimsPrincipalFactory principalFactory,
            TenantManager tenantManager,
            TokenAuthConfiguration configuration,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _otpChallengeService = otpChallengeService;
            _userManager = userManager;
            _principalFactory = principalFactory;
            _tenantManager = tenantManager;
            _configuration = configuration;
            _unitOfWorkManager = unitOfWorkManager;
        }

        [HttpPost]
        public async Task RequestOtp([FromBody] RequestOtpModel model)
        {
            await _otpChallengeService.RequestAsync(model.PhoneNumber);
        }

        [HttpPost]
        [UnitOfWork]
        public async Task<AuthenticateResultModel> VerifyOtp([FromBody] VerifyOtpModel model)
        {
            await _otpChallengeService.VerifyAsync(model.PhoneNumber, model.Code);

            var defaultTenant = await _tenantManager.FindByTenancyNameAsync(AbpTenantBase.DefaultTenantName);

            User user;
            ClaimsIdentity identity;

            using (_unitOfWorkManager.Current.SetTenantId(defaultTenant.Id))
            {
                user = await _userManager.FindByNameAsync(model.PhoneNumber);
                if (user == null)
                {
                    user = new User
                    {
                        TenantId = defaultTenant.Id,
                        UserName = model.PhoneNumber,
                        Name = model.PhoneNumber,
                        Surname = model.PhoneNumber,
                        // Shoppers have no email - AbpUser.SetNormalizedNames()
                        // (and the entity's [Required] EmailAddress column)
                        // need a non-null value, so synthesize a placeholder;
                        // it's never shown or used, login is phone+OTP only.
                        EmailAddress = $"{model.PhoneNumber}@shopper.prizzoo.local",
                        PhoneNumber = model.PhoneNumber,
                        IsPhoneNumberConfirmed = true,
                        IsActive = true,
                        Roles = new List<UserRole>(),
                    };
                    user.SetNormalizedNames();

                    // No password is ever set by the shopper - OTP is the
                    // only login path for this role, so a random unusable
                    // password just satisfies Identity's storage requirement.
                    CheckErrors(await _userManager.CreateAsync(user, EnterpriseBase.Authorization.Users.User.CreateRandomPassword()));
                    CheckErrors(await _userManager.AddToRoleAsync(user, StaticRoleNames.Tenants.Shopper));
                    await _unitOfWorkManager.Current.SaveChangesAsync();
                }

                identity = (ClaimsIdentity)(await _principalFactory.CreateAsync(user)).Identity;
            }

            // Shoppers get a long-lived session (see AppConsts.ShopperAccessTokenExpiration)
            // rather than the shared 1-day Admin/ShopOwner token - they should
            // stay logged in until they actually log out, not be forced to
            // re-verify their phone number daily.
            var accessToken = CreateAccessToken(CreateJwtClaims(identity), AppConsts.ShopperAccessTokenExpiration);

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)AppConsts.ShopperAccessTokenExpiration.TotalSeconds,
                UserId = user.Id,
            };
        }
    }
}
