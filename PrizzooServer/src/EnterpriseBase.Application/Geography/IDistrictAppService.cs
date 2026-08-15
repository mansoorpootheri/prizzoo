using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Geography.Dto;
using System.Threading.Tasks;

namespace EnterpriseBase.Geography
{
    public interface IDistrictAppService : IApplicationService
    {
        Task<PagedResultDto<GetDistrictForViewDto>> GetAll(GetAllDistrictsInput input);
        Task<GetDistrictForViewDto> GetDistrictForView(int id);
        Task<GetDistrictForEditOutput> GetDistrictForEdit(EntityDto input);
        Task CreateOrEdit(CreateDistrictEditDto input);
        Task Delete(EntityDto input);
        Task<ListResultDto<DistrictDto>> GetDistrictsByStateAsync(int stateId);
    }
}