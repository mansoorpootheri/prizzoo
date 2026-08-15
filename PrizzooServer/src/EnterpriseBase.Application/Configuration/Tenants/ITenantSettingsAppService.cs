using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using EnterpriseBase.Configuration.Tenants.Dto;

namespace EnterpriseBase.Configuration.Tenants
{
    public interface ITenantSettingsAppService : IApplicationService
    {
        Task<TenantSettingsEditDto> GetAllSettings();
        Task UpdateAllSettings(TenantSettingsEditDto input);
        Task<UploadCompanyLogoOutput> UploadCompanyLogo(UploadCompanyLogoInput input);
        Task DeleteCompanyLogo();
        Task<GetCompanyLogoOutput> GetCompanyLogo();
    }
}
