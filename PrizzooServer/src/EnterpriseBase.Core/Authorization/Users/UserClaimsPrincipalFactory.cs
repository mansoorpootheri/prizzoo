using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using EnterpriseBase.Authorization.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EnterpriseBase.Authorization.Users;

public class UserClaimsPrincipalFactory : AbpUserClaimsPrincipalFactory<User, Role>
{
    private readonly IRepository<UserBranchMapping,long> _userBranchMappingRepository;
    public UserClaimsPrincipalFactory(
        UserManager userManager,
        RoleManager roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        IUnitOfWorkManager unitOfWorkManager,
        IRepository<UserBranchMapping, long> userBranchMappingRepository)
        : base(
              userManager,
              roleManager,
              optionsAccessor,
              unitOfWorkManager)
    {
        _userBranchMappingRepository = userBranchMappingRepository;
    }

    public override async Task<ClaimsPrincipal> CreateAsync(User user)
    {
        var branchids = _userBranchMappingRepository.GetAll().Where(x => x.UserId == user.Id).ToList();
        var claim = await base.CreateAsync(user);
        var claims = new List<string>();

        foreach (var item in branchids)
        {
            claims.Add(item.BranchId.ToString());
        }
        var branches = string.Join(",", claims);
        if (branches.Length > 0)
        {
            claim.Identities.First().AddClaim(new Claim("BranchIds", branches));
        }
        return claim;
    }
}
