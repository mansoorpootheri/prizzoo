using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using EnterpriseBase.Branches;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Authorization.Users;

[Table("UserBranchMappings")]
public class UserBranchMapping : CreationAuditedEntity<long>, IMayHaveTenant
{
    public int? TenantId { get; set; }

    public long UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    public int BranchId { get; set; }
    [ForeignKey("BranchId")]
    public virtual Branch Branch { get; set; }
}