using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Branches.Dto;
using EnterpriseBase.Roles.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Branches
{
    public interface IBranchAppService : IApplicationService
    {
        Task<PagedResultDto<GetBranchForViewDto>> GetAll(GetAllBranchesInput input);
        Task<GetBranchForViewDto> GetBranchForView(int id);
        Task<GetBranchForEditOutput> GetBranchForEdit(EntityDto input);
        Task CreateOrEdit(CreateBranchEditDto input);
        Task<List<ComboboxItemDto>> GetBranchesForCombobox();
        Task<ListResultDto<BranchDto>> GetAllBranches();
    }
}