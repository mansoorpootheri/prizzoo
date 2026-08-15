using Abp.Authorization;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;

namespace EnterpriseBase.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
