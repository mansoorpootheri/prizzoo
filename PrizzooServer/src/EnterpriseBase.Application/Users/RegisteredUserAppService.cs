using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using EnterpriseBase.Authorization;
using EnterpriseBase.Authorization.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseBase.Application.Users
{
    public class RegisteredUserDto
    {
        public long Id { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsActive { get; set; }
    }

    public interface IRegisteredUserAppService : IApplicationService
    {
        Task<List<RegisteredUserDto>> GetShoppersAsync();
    }

    /// <summary>
    /// Read-only admin view of every shopper account created via OTP login
    /// (see OtpAuthController.VerifyOtp) - distinct from AdminAppService,
    /// which lists/manages the Admin-role accounts it itself creates.
    /// Shoppers self-register, so there's nothing here to add/edit.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_RegisteredUsers)]
    public class RegisteredUserAppService : EnterpriseBaseAppServiceBase, IRegisteredUserAppService
    {
        private readonly IRepository<Role> _roleRepository;

        public RegisteredUserAppService(IRepository<Role> roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [UnitOfWork]
        public virtual async Task<List<RegisteredUserDto>> GetShoppersAsync()
        {
            var defaultTenant = await TenantManager.FindByTenancyNameAsync(AbpTenantBase.DefaultTenantName);
            if (defaultTenant == null)
                return new List<RegisteredUserDto>();

            using (CurrentUnitOfWork.SetTenantId(defaultTenant.Id))
            {
                var shopperRole = await _roleRepository.GetAll()
                    .FirstOrDefaultAsync(r => r.TenantId == defaultTenant.Id && r.Name == StaticRoleNames.Tenants.Shopper);
                if (shopperRole == null)
                    return new List<RegisteredUserDto>();

                var shoppers = await UserManager.Users
                    .Where(u => u.TenantId == defaultTenant.Id)
                    .Where(u => u.Roles.Any(r => r.RoleId == shopperRole.Id))
                    .OrderByDescending(u => u.CreationTime)
                    .ToListAsync();

                return shoppers.Select(u => new RegisteredUserDto
                {
                    Id           = u.Id,
                    PhoneNumber  = u.PhoneNumber,
                    CreationTime = u.CreationTime,
                    IsActive     = u.IsActive,
                }).ToList();
            }
        }
    }
}
