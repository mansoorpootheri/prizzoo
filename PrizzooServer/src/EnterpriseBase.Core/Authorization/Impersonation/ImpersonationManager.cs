using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.UI;
using EnterpriseBase.Authorization.Users;

namespace EnterpriseBase.Authorization.Impersonation
{
    public class ImpersonationManager : EnterpriseBaseDomainServiceBase, IImpersonationManager
    {
        public IAbpSession AbpSession { get; set; }

        private readonly ICacheManager _cacheManager;
        private readonly UserManager _userManager;
        private readonly UserClaimsPrincipalFactory _principalFactory;

        public ImpersonationManager(
            ICacheManager cacheManager,
            UserManager userManager,
            UserClaimsPrincipalFactory principalFactory)
        {
            _cacheManager = cacheManager;
            _userManager = userManager;
            _principalFactory = principalFactory;

            AbpSession = NullAbpSession.Instance;
        }

        public async Task<UserAndIdentity> GetImpersonatedUserAndIdentity(string impersonationToken)
        {
            var cacheItem = await _cacheManager.GetImpersonationCache().GetOrDefaultAsync(impersonationToken);
            if (cacheItem == null)
            {
                throw new UserFriendlyException(L("ImpersonationTokenErrorMessage"));
            }

            // Skip tenant check for cross-tenant impersonation scenarios:
            // - Host (null tenant) impersonating a tenant user
            // - Impersonated tenant user switching back to host
            if (AbpSession.TenantId.HasValue && !cacheItem.IsBackToImpersonator && 
                cacheItem.TargetTenantId.HasValue && AbpSession.TenantId == cacheItem.TargetTenantId)
            {
                // Same-tenant impersonation - validate
            }
            else if (!cacheItem.IsBackToImpersonator && AbpSession.TenantId.HasValue && 
                     cacheItem.TargetTenantId != AbpSession.TenantId)
            {
                throw new UserFriendlyException(L("DifferentTenantImpersonationErrorMessage"));
            }

            User user;
            ClaimsIdentity identity;

            // Set tenant context for both user lookup AND identity/claims creation
            using (UnitOfWorkManager.Current.SetTenantId(cacheItem.TargetTenantId))
            {
                //Get the user from tenant
                user = await _userManager.FindByIdAsync(cacheItem.TargetUserId.ToString());

                if (user == null)
                {
                    throw new UserFriendlyException("Target user not found.");
                }

                //Create identity (must be inside tenant scope for correct claims)
                identity = await GetClaimsIdentityFromCache(user, cacheItem);
            }

            //Remove the cache item to prevent re-use
            await _cacheManager.GetImpersonationCache().RemoveAsync(impersonationToken);

            return new UserAndIdentity(user, identity);
        }

        private async Task<ClaimsIdentity> GetClaimsIdentityFromCache(User user, ImpersonationCacheItem cacheItem)
        {
            var identity = (ClaimsIdentity) (await _principalFactory.CreateAsync(user)).Identity;

            if (!cacheItem.IsBackToImpersonator)
            {
                //Add claims for audit logging
                if (cacheItem.ImpersonatorTenantId.HasValue)
                {
                    identity.AddClaim(new Claim(AbpClaimTypes.ImpersonatorTenantId,
                        cacheItem.ImpersonatorTenantId.Value.ToString(CultureInfo.InvariantCulture)));
                }

                identity.AddClaim(new Claim(AbpClaimTypes.ImpersonatorUserId,
                    cacheItem.ImpersonatorUserId.ToString(CultureInfo.InvariantCulture)));
            }

            return identity;
        }

        public Task<string> GetImpersonationToken(long userId, int? tenantId)
        {
            if (AbpSession.ImpersonatorUserId.HasValue)
            {
                throw new UserFriendlyException(L("CascadeImpersonationErrorMessage"));
            }

            if (AbpSession.TenantId.HasValue)
            {
                if (!tenantId.HasValue)
                {
                    throw new UserFriendlyException(L("FromTenantToHostImpersonationErrorMessage"));
                }

                if (tenantId.Value != AbpSession.TenantId.Value)
                {
                    throw new UserFriendlyException(L("DifferentTenantImpersonationErrorMessage"));
                }
            }

            return GenerateImpersonationTokenAsync(tenantId, userId, false);
        }

        public Task<string> GetBackToImpersonatorToken()
        {
            if (!AbpSession.ImpersonatorUserId.HasValue)
            {
                throw new UserFriendlyException(L("NotImpersonatedLoginErrorMessage"));
            }

            return GenerateImpersonationTokenAsync(AbpSession.ImpersonatorTenantId, AbpSession.ImpersonatorUserId.Value, true);
        }

        private void CheckCurrentTenant(int? tenantId)
        {
            if (AbpSession.TenantId != tenantId)
            {
                throw new Exception($"Current tenant is different than given tenant. AbpSession.TenantId: {AbpSession.TenantId}, given tenantId: {tenantId}");
            }
        }

        private async Task<string> GenerateImpersonationTokenAsync(int? tenantId, long userId, bool isBackToImpersonator)
        {
            //Create a cache item
            var cacheItem = new ImpersonationCacheItem(
                tenantId,
                userId,
                isBackToImpersonator
            );

            if (!isBackToImpersonator)
            {
                cacheItem.ImpersonatorTenantId = AbpSession.TenantId;
                cacheItem.ImpersonatorUserId = AbpSession.GetUserId();
            }

            //Create a random token and save to the cache
            var token = Guid.NewGuid().ToString();

            await _cacheManager
                .GetImpersonationCache()
                .SetAsync(token, cacheItem, TimeSpan.FromMinutes(1));

            return token;
        }
    }
}
