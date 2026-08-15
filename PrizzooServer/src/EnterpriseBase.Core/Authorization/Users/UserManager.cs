using Abp;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Abp.UI;
using EnterpriseBase.Authorization.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Authorization.Users;

public class UserManager : AbpUserManager<Role, User>
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public UserManager(
      RoleManager roleManager,
      UserStore store,
      IOptions<IdentityOptions> optionsAccessor,
      IPasswordHasher<User> passwordHasher,
      IEnumerable<IUserValidator<User>> userValidators,
      IEnumerable<IPasswordValidator<User>> passwordValidators,
      ILookupNormalizer keyNormalizer,
      IdentityErrorDescriber errors,
      IServiceProvider services,
      ILogger<UserManager<User>> logger,
      IPermissionManager permissionManager,
      IUnitOfWorkManager unitOfWorkManager,
      ICacheManager cacheManager,
      IRepository<OrganizationUnit, long> organizationUnitRepository,
      IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
      IOrganizationUnitSettings organizationUnitSettings,
      ISettingManager settingManager,
      IRepository<UserLogin, long> userLoginRepository)
      : base(
          roleManager,
          store,
          optionsAccessor,
          passwordHasher,
          userValidators,
          passwordValidators,
          keyNormalizer,
          errors,
          services,
          logger,
          permissionManager,
          unitOfWorkManager,
          cacheManager,
          organizationUnitRepository,
          userOrganizationUnitRepository,
          organizationUnitSettings,
          settingManager,
          userLoginRepository)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }
    public virtual async Task<User> GetUserOrNullAsync(UserIdentifier userIdentifier)
    {
        return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.SetTenantId(userIdentifier.TenantId))
            {
                return await FindByIdAsync(userIdentifier.UserId.ToString());
            }
        });
    }

    public async Task<User> GetUserAsync(UserIdentifier userIdentifier)
    {
        var user = await GetUserOrNullAsync(userIdentifier);
        if (user == null)
        {
            throw new UserFriendlyException("Unknown user or user identifier");
        }

        return user;
    }

    public async Task<int?> TryGetTenantIdOfUser(string userEmail)
    {
        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
            var user = await Users.SingleOrDefaultAsync(u => u.EmailAddress == userEmail.Trim());
            return user?.TenantId;
        }
    }
}
