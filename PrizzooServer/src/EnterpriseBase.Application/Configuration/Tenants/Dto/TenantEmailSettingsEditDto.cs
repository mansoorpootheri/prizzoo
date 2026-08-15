using Abp.Auditing;
using EnterpriseBase.Configuration.Dto;

namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class TenantEmailSettingsEditDto : EmailSettingsEditDto
    {
        public bool UseHostDefaultEmailSettings { get; set; }
    }
}