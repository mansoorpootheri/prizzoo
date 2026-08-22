using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.UI;
using EnterpriseBase.Authorization;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Admin
{
    public class AdminUserDto
    {
        public long Id { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class AddAdminDto
    {
        [Required]
        [Phone]
        [MaxLength(30)]
        public string PhoneNumber { get; set; }
    }

    public interface IAdminAppService : IApplicationService
    {
        Task<List<AdminUserDto>> GetAdminsAsync();
        Task AddAdminAsync(AddAdminDto input);
    }

    /// <summary>
    /// Lets a logged-in admin add further admin phone numbers, so there's a
    /// way to grow beyond the single seeded AppConsts.InitialAdminPhoneNumber
    /// account without touching the database directly. Mirrors
    /// OtpAuthController.VerifyOtp's find-or-create-by-phone-number shape,
    /// just admin-triggered instead of happening on first OTP login.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Admins)]
    public class AdminAppService : EnterpriseBaseAppServiceBase, IAdminAppService
    {
        private readonly IRepository<Role> _roleRepository;

        public AdminAppService(IRepository<Role> roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [UnitOfWork]
        public virtual async Task<List<AdminUserDto>> GetAdminsAsync()
        {
            var defaultTenant = await TenantManager.FindByTenancyNameAsync(AbpTenantBase.DefaultTenantName);
            if (defaultTenant == null)
                return new List<AdminUserDto>();

            using (CurrentUnitOfWork.SetTenantId(defaultTenant.Id))
            {
                var adminRole = await _roleRepository.GetAll()
                    .FirstOrDefaultAsync(r => r.TenantId == defaultTenant.Id && r.Name == StaticRoleNames.Tenants.Admin);
                if (adminRole == null)
                    return new List<AdminUserDto>();

                var admins = await UserManager.Users
                    .Where(u => u.TenantId == defaultTenant.Id)
                    .Where(u => u.Roles.Any(r => r.RoleId == adminRole.Id))
                    .OrderBy(u => u.CreationTime)
                    .ToListAsync();

                return admins.Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    PhoneNumber = u.PhoneNumber,
                    CreationTime = u.CreationTime,
                }).ToList();
            }
        }

        [UnitOfWork]
        public virtual async Task AddAdminAsync(AddAdminDto input)
        {
            var defaultTenant = await TenantManager.FindByTenancyNameAsync(AbpTenantBase.DefaultTenantName);
            if (defaultTenant == null)
                throw new UserFriendlyException("Default tenant not found.");

            using (CurrentUnitOfWork.SetTenantId(defaultTenant.Id))
            {
                var user = await UserManager.FindByNameAsync(input.PhoneNumber);
                if (user == null)
                {
                    user = new User
                    {
                        TenantId = defaultTenant.Id,
                        UserName = input.PhoneNumber,
                        Name = input.PhoneNumber,
                        Surname = input.PhoneNumber,
                        // No email is ever shown/used - login is phone+OTP only,
                        // same synthetic-email convention as OtpAuthController's
                        // shopper accounts and the seeded initial admin.
                        EmailAddress = $"{input.PhoneNumber}@admin.prizzoo.local",
                        PhoneNumber = input.PhoneNumber,
                        IsPhoneNumberConfirmed = true,
                        IsEmailConfirmed = true,
                        IsActive = true,
                        Roles = new List<UserRole>(),
                    };
                    user.SetNormalizedNames();

                    // No password is ever set - OTP is the only login path for
                    // this account, a random unusable password just satisfies
                    // Identity's storage requirement.
                    CheckErrors(await UserManager.CreateAsync(user, User.CreateRandomPassword()));
                }

                if (!await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Admin))
                {
                    CheckErrors(await UserManager.AddToRoleAsync(user, StaticRoleNames.Tenants.Admin));
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }
    }
}
