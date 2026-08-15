using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Employees.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Employees
{
    public interface IEmployeeTypeAppService : IApplicationService
    {
        Task<PagedResultDto<GetEmployeeTypeForViewDto>> GetAll(GetAllEmployeeTypesInput input);
        Task<GetEmployeeTypeForViewDto> GetEmployeeTypeForView(int id);
        Task<GetEmployeeTypeForEditOutput> GetEmployeeTypeForEdit(EntityDto input);
        Task CreateOrEdit(CreateEmployeeTypeEditDto input);
        Task<List<ComboboxItemDto>> GetEmployeeTypesForCombobox();
    }
}