using Abp.Authorization.Users;
using Abp.Extensions;
using System;
using System.Collections.Generic;

namespace EnterpriseBase.Authorization.Users;

public class User : AbpUser<User>
{
    public const string DefaultPassword = "123qwe";

    public virtual ICollection<UserBranchMapping> UserBranchMappings { get; set; }

    public static string CreateRandomPassword()
    {
        // A plain GUID hex slice is all lowercase+digits, which fails the
        // default Identity password policy (needs upper + non-alphanumeric
        // too) - prefix guarantees every required character class is present
        // regardless of the random portion.
        return "Aa1!" + Guid.NewGuid().ToString("N").Truncate(12);
    }

    public static User CreateTenantAdminUser(int tenantId, string emailAddress)
    {
        var user = new User
        {
            TenantId = tenantId,
            UserName = AdminUserName,
            Name = AdminUserName,
            Surname = AdminUserName,
            EmailAddress = emailAddress,
            Roles = new List<UserRole>()
        };

        user.SetNormalizedNames();

        return user;
    }
}
