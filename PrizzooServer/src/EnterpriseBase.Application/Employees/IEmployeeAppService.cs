using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Employees.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Employees
{
    public interface IEmployeeAppService : IApplicationService
    {
        Task<PagedResultDto<GetEmployeeForViewDto>> GetAll(GetAllEmployeesInput input);
        Task<GetEmployeeForViewDto> GetEmployeeForView(long id);
        Task<GetEmployeeForEditOutput> GetEmployeeForEdit(EntityDto<long> input);
        Task CreateOrEdit(CreateEmployeeEditDto input);
        Task<List<ComboboxItemDto>> GetUsers();
        Task<List<ComboboxItemDto>> GetEmployeeTypesForCombobox();
        Task<List<ComboboxItemDto>> GetBranchesForCombobox();
    }
}