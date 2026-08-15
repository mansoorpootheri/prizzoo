using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Authorization.Accounts.Dto
{
    public class ImpersonateTenantInput
    {
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// If not provided, the tenant's admin user will be used automatically.
        /// </summary>
        public long? UserId { get; set; }
    }
}