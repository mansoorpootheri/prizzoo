using Abp.Application.Editions;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.IdentityFramework;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Zero.Configuration;
using EnterpriseBase.Authorization.Accounts.Dto;
using EnterpriseBase.Authorization.Delegation;
using EnterpriseBase.Authorization.Impersonation;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Authorization.Accounts;

public class AccountAppService : EnterpriseBaseAppServiceBase, IAccountAppService
{
    // from: http://regexlib.com/REDetails.aspx?regexp_id=1923
    public const string PasswordRegex = "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

    private readonly UserRegistrationManager _userRegistrationManager;
    private readonly IImpersonationManager _impersonationManager;
    private readonly IUserDelegationManager _userDelegationManager;
    private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;
    private readonly RoleManager _roleManager;
    private readonly IRepository<Edition, int> _editionRepository;

    public AccountAppService(
        UserRegistrationManager userRegistrationManager, 
        IImpersonationManager impersonationManager, 
        IUserDelegationManager userDelegationManager,
        IAbpZeroDbMigrator abpZeroDbMigrator,
        RoleManager roleManager,
        IRepository<Edition, int> editionRepository)
    {
        _userRegistrationManager = userRegistrationManager;
        _impersonationManager = impersonationManager;
        _userDelegationManager = userDelegationManager;
        _abpZeroDbMigrator = abpZeroDbMigrator;
        _roleManager = roleManager;
        _editionRepository = editionRepository;
    }

    public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
    {
        var tenant = await TenantManager.FindByTenancyNameAsync(input.TenancyName);
        if (tenant == null)
        {
            return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
        }

        if (!tenant.IsActive)
        {
            return new IsTenantAvailableOutput(TenantAvailabilityState.InActive);
        }

        return new IsTenantAvailableOutput(TenantAvailabilityState.Available, tenant.Id);
    }

    public async Task<RegisterOutput> Register(RegisterInput input)
    {
        var user = await _userRegistrationManager.RegisterAsync(
            input.Name,
            input.Surname,
            input.EmailAddress,
            input.UserName,
            input.Password,
            true // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
        );

        var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);

        return new RegisterOutput
        {
            CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin)
        };
    }

    public async Task<TenantSignupOutput> TenantSignup(TenantSignupInput input)
    {
        try
        {
            // Generate unique tenancy name from company name
            var tenancyName = GenerateTenancyName(input.CompanyName);
            
            // Check if tenancy name already exists
            var existingTenant = await TenantManager.FindByTenancyNameAsync(tenancyName);
            if (existingTenant != null)
            {
                // Add random suffix if exists
                tenancyName = $"{tenancyName}{new Random().Next(100, 999)}";
            }

            // Create tenant
            var tenant = new Tenant(tenancyName, input.CompanyName)
            {
                IsActive = true,
                IsInTrialPeriod = true,
                SubscriptionEndDateUtc = DateTime.UtcNow.AddDays(30) // 30-day trial
            };

            // Assign Free trial edition
            var freeEdition = await _editionRepository
                .FirstOrDefaultAsync(e => e.Name == "Free");
            if (freeEdition != null)
            {
                tenant.EditionId = freeEdition.Id;
            }

            await TenantManager.CreateAsync(tenant);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Create tenant database
            _abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);

            // Switch to tenant context - same as TenantAppService
            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                // Create static roles for new tenant - EXACTLY like TenantAppService
                CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));
                await CurrentUnitOfWork.SaveChangesAsync();

                // Grant all permissions to admin role - EXACTLY like TenantAppService
                var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                await _roleManager.GrantAllPermissionsAsync(adminRole);

                // Create admin user for the tenant - EXACTLY like TenantAppService
                var adminUser = User.CreateTenantAdminUser(tenant.Id, input.Email);
                adminUser.Name = input.FirstName;
                adminUser.Surname = input.LastName;
                
                await UserManager.InitializeOptionsAsync(tenant.Id);
                CheckErrors(await UserManager.CreateAsync(adminUser, input.Password));
                await CurrentUnitOfWork.SaveChangesAsync();

                // Assign admin user to role - EXACTLY like TenantAppService
                CheckErrors(await UserManager.AddToRoleAsync(adminUser, adminRole.Name));
                await CurrentUnitOfWork.SaveChangesAsync();

                // Seed branch and employee types for new tenant
                await CurrentUnitOfWork.SaveChangesAsync();

                return new TenantSignupOutput
                {
                    Success = true,
                    Message = "Account created successfully",
                    TenantId = tenant.Id,
                    UserId = adminUser.Id,
                    CanLogin = adminUser.IsActive,
                    TenancyName = tenant.TenancyName
                };
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Error creating tenant signup", ex);
            return new TenantSignupOutput
            {
                Success = false,
                Message = "Failed to create account. Please try again."
            };
        }
    }

    private new void CheckErrors(Microsoft.AspNetCore.Identity.IdentityResult identityResult)
    {
        identityResult.CheckErrors(LocalizationManager);
    }

    private string GenerateTenancyName(string companyName)
    {
        // Remove special characters and spaces, convert to lowercase
        var tenancyName = System.Text.RegularExpressions.Regex.Replace(
            companyName.ToLowerInvariant(), 
            @"[^a-z0-9]", 
            ""
        );
        
        // Ensure it's not empty and has reasonable length
        if (string.IsNullOrEmpty(tenancyName) || tenancyName.Length < 3)
        {
            tenancyName = "company" + new Random().Next(1000, 9999);
        }
        else if (tenancyName.Length > 20)
        {
            tenancyName = tenancyName.Substring(0, 20);
        }
        
        return tenancyName;
    }



    [AbpAuthorize(PermissionNames.Pages_Administration_Users_Impersonation)]
    public virtual async Task<ImpersonateOutput> ImpersonateUser(ImpersonateUserInput input)
    {
        return new ImpersonateOutput
        {
            ImpersonationToken =
                await _impersonationManager.GetImpersonationToken(input.UserId, AbpSession.TenantId),
            TenancyName = await GetTenancyNameOrNullAsync(input.TenantId)
        };
    }

    [AbpAuthorize(PermissionNames.Pages_Tenants_Impersonation)]
    public virtual async Task<ImpersonateOutput> ImpersonateTenant(ImpersonateTenantInput input)
    {
        var targetUserId = input.UserId;

        // If no UserId provided, find the admin user of the tenant
        if (!targetUserId.HasValue)
        {
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                var adminUser = await UserManager.Users
                    .Where(u => u.TenantId == input.TenantId && u.UserName == Abp.Authorization.Users.AbpUserBase.AdminUserName)
                    .FirstOrDefaultAsync();

                if (adminUser == null)
                {
                    throw new UserFriendlyException("Admin user not found for this tenant.");
                }

                targetUserId = adminUser.Id;
            }
        }

        return new ImpersonateOutput
        {
            ImpersonationToken = await _impersonationManager.GetImpersonationToken(targetUserId.Value, input.TenantId),
            TenancyName = await GetTenancyNameOrNullAsync(input.TenantId)
        };
    }

    public virtual async Task<ImpersonateOutput> DelegatedImpersonate(DelegatedImpersonateInput input)
    {
        var userDelegation = await _userDelegationManager.GetAsync(input.UserDelegationId);
        if (userDelegation.TargetUserId != AbpSession.GetUserId())
        {
            throw new UserFriendlyException("User delegation error.");
        }

        return new ImpersonateOutput
        {
            ImpersonationToken =
                await _impersonationManager.GetImpersonationToken(userDelegation.SourceUserId,
                    userDelegation.TenantId),
            TenancyName = await GetTenancyNameOrNullAsync(userDelegation.TenantId)
        };
    }

    public virtual async Task<ImpersonateOutput> BackToImpersonator()
    {
        return new ImpersonateOutput
        {
            ImpersonationToken = await _impersonationManager.GetBackToImpersonatorToken(),
            TenancyName = await GetTenancyNameOrNullAsync(AbpSession.ImpersonatorTenantId)
        };
    }

    private async Task<Tenant> GetActiveTenantAsync(int tenantId)
    {
        var tenant = await TenantManager.FindByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new UserFriendlyException(L("UnknownTenantId{0}", tenantId));
        }

        if (!tenant.IsActive)
        {
            throw new UserFriendlyException(L("TenantIdIsNotActive{0}", tenantId));
        }

        return tenant;
    }

    private async Task<string> GetTenancyNameOrNullAsync(int? tenantId)
    {
        return tenantId.HasValue ? (await GetActiveTenantAsync(tenantId.Value)).TenancyName : null;
    }
}
