using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Geography.Dto;
using System.Threading.Tasks;

namespace EnterpriseBase.Geography
{
    public interface IStateAppService : IApplicationService
    {
        Task<PagedResultDto<GetStateForViewDto>> GetAll(GetAllStatesInput input);
        Task<GetStateForViewDto> GetStateForView(int id);
        Task<GetStateForEditOutput> GetStateForEdit(EntityDto input);
        Task CreateOrEdit(CreateStateEditDto input);
        Task Delete(EntityDto input);
        Task<ListResultDto<StateDto>> GetStatesByCountryAsync(int countryId);
    }
}