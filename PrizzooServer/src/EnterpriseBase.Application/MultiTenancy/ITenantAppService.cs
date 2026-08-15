using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Application.Editions.Dto;
using EnterpriseBase.MultiTenancy.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
    Task<GetTenantFeaturesEditOutput> GetTenantFeaturesForEdit(EntityDto input);

    Task UpdateTenantFeatures(UpdateTenantFeaturesInput input);

    Task ResetTenantSpecificFeatures(EntityDto input);

    Task<List<SubscribableEditionComboboxItemDto>> GetEditionsForCombobox();

    Task<List<TenantUserLookupDto>> GetTenantUsers(EntityDto input);
}