using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using EnterpriseBase.Authorization;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace EnterpriseBase.EntityFrameworkCore.Seed.Tenants;

public class TenantRoleAndUserBuilder
{
    private readonly EnterpriseBaseDbContext _context;
    private readonly int _tenantId;

    public TenantRoleAndUserBuilder(EnterpriseBaseDbContext context, int tenantId)
    {
        _context = context;
        _tenantId = tenantId;
    }

    public void Create()
    {
        CreateRolesAndUsers();
        CreateShopperRole();
    }

    private void CreateRolesAndUsers()
    {
        // Admin role

        var adminRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Admin);
        if (adminRole == null)
        {
            adminRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin) { IsStatic = true }).Entity;
            _context.SaveChanges();
        }

        // Grant all permissions to admin role

        var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
            .OfType<RolePermissionSetting>()
            .Where(p => p.TenantId == _tenantId && p.RoleId == adminRole.Id)
            .Select(p => p.Name)
            .ToList();

        var permissions = PermissionFinder
            .GetAllPermissions(new EnterpriseBaseAuthorizationProvider())
            .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant) &&
                        !grantedPermissions.Contains(p.Name))
            .ToList();

        if (permissions.Any())
        {
            _context.Permissions.AddRange(
                permissions.Select(permission => new RolePermissionSetting
                {
                    TenantId = _tenantId,
                    Name = permission.Name,
                    IsGranted = true,
                    RoleId = adminRole.Id
                })
            );
            _context.SaveChanges();
        }

        // Admin user - phone+OTP login only (see OtpAuthController), keyed
        // by UserName == PhoneNumber == EnterpriseBaseConsts.InitialAdminPhoneNumber,
        // same convention as the shopper accounts OtpAuthController.VerifyOtp
        // creates on first login. No password is ever checked for this
        // account; the hash below is a throwaway unusable value.

        var adminUser = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == EnterpriseBaseConsts.InitialAdminPhoneNumber);
        if (adminUser == null)
        {
            var phoneNumber = EnterpriseBaseConsts.InitialAdminPhoneNumber;
            adminUser = new User
            {
                TenantId = _tenantId,
                UserName = phoneNumber,
                Name = phoneNumber,
                Surname = phoneNumber,
                EmailAddress = $"{phoneNumber}@admin.prizzoo.local",
                PhoneNumber = phoneNumber,
                IsPhoneNumberConfirmed = true,
                IsEmailConfirmed = true,
                IsActive = true,
                Roles = new List<UserRole>(),
            };
            adminUser.SetNormalizedNames();
            adminUser.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(adminUser, User.CreateRandomPassword());

            _context.Users.Add(adminUser);
            _context.SaveChanges();
        }

        // Ensure this account has the Admin role even if it already existed
        // under a different role - e.g. this phone number logged in as a
        // Shopper (via OtpAuthController) before being designated the
        // initial admin, in which case the block above found an existing
        // user and never touched its roles. Checked separately from
        // creation so this self-heals on every startup regardless of when
        // the account first appeared.
        var alreadyAdmin = _context.UserRoles.IgnoreQueryFilters()
            .Any(ur => ur.TenantId == _tenantId && ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
        if (!alreadyAdmin)
        {
            _context.UserRoles.Add(new UserRole(_tenantId, adminUser.Id, adminRole.Id));
            _context.SaveChanges();
        }
    }

    private void CreateShopperRole()
    {
        // Not IsDefault: role assignment happens on first successful OTP
        // verification (OtpAuthController.VerifyOtp), not at registration -
        // there is no separate shopper registration flow.
        var shopperRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Shopper);
        if (shopperRole == null)
        {
            shopperRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Shopper, StaticRoleNames.Tenants.Shopper) { IsStatic = true }).Entity;
            _context.SaveChanges();
        }

        GrantPermissions(shopperRole.Id, PermissionNames.Pages_Shopper);
    }

    private void GrantPermissions(int roleId, params string[] permissionNames)
    {
        var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
            .OfType<RolePermissionSetting>()
            .Where(p => p.TenantId == _tenantId && p.RoleId == roleId)
            .Select(p => p.Name)
            .ToList();

        var toGrant = permissionNames
            .Where(name => !grantedPermissions.Contains(name))
            .ToList();

        if (toGrant.Any())
        {
            _context.Permissions.AddRange(
                toGrant.Select(name => new RolePermissionSetting
                {
                    TenantId = _tenantId,
                    Name = name,
                    IsGranted = true,
                    RoleId = roleId
                })
            );
            _context.SaveChanges();
        }
    }
}