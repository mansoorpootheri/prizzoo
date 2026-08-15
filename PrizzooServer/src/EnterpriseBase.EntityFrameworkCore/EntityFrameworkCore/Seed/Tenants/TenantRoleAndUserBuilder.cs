using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using EnterpriseBase.Authorization;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Branches;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        new DefaultBranchCreator(_context, _tenantId).Create();
        new DefaultEmployeeTypesCreator(_context, _tenantId).Create();
        SeedAdminUserBranchMapping();
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

        // Admin user - Check both username and tenant to avoid duplicates

        var adminUser = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == AbpUserBase.AdminUserName);
        if (adminUser == null)
        {
            // Get tenant info to create unique email
            var tenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.Id == _tenantId);
            var tenantName = tenant?.TenancyName ?? "defaulttenant";
            var adminEmail = $"admin@{tenantName}.com";
            
            // Ensure email uniqueness across all tenants
            var emailExists = _context.Users.IgnoreQueryFilters().Any(u => u.EmailAddress == adminEmail);
            if (emailExists)
            {
                adminEmail = $"admin@{tenantName}-{_tenantId}.com";
            }

            adminUser = User.CreateTenantAdminUser(_tenantId, adminEmail);
            adminUser.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(adminUser, "123qwe");
            adminUser.IsEmailConfirmed = true;
            adminUser.IsActive = true;

            _context.Users.Add(adminUser);
            _context.SaveChanges();

            // Assign Admin role to admin user
            _context.UserRoles.Add(new UserRole(_tenantId, adminUser.Id, adminRole.Id));
            _context.SaveChanges();
        }
    }

    private void SeedAdminUserBranchMapping()
    {
        var adminUser = _context.Users.IgnoreQueryFilters()
            .FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == AbpUserBase.AdminUserName);

        var branch = _context.Branches.IgnoreQueryFilters()
            .FirstOrDefault(b => b.TenantId == _tenantId && b.IsHeadOffice);

        if (adminUser == null || branch == null) return;

        var exists = _context.Set<UserBranchMapping>()
            .IgnoreQueryFilters()
            .Any(m => m.UserId == adminUser.Id && m.BranchId == branch.Id);

        if (!exists)
        {
            _context.Set<UserBranchMapping>().Add(new UserBranchMapping
            {
                TenantId = _tenantId,
                UserId   = adminUser.Id,
                BranchId = branch.Id
            });
            _context.SaveChanges();
        }
    }
}