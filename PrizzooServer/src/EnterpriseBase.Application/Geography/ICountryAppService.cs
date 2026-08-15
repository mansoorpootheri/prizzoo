using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Geography.Dto;
using System.Threading.Tasks;

namespace EnterpriseBase.Geography
{
    public interface ICountryAppService : IApplicationService
    {
        Task<PagedResultDto<GetCountryForViewDto>> GetAll(GetAllCountriesInput input);
        Task<GetCountryForViewDto> GetCountryForView(int id);
        Task<GetCountryForEditOutput> GetCountryForEdit(EntityDto input);
        Task CreateOrEdit(CreateCountryEditDto input);
        Task Delete(EntityDto input);
    }
}