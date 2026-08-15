using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Application.Editions.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Editions
{
    public interface IEditionAppService : IApplicationService
    {
        Task<ListResultDto<EditionListDto>> GetEditions();
        Task<GetEditionEditOutput> GetEditionForEdit(NullableIdDto input);
        Task CreateOrUpdateEdition(CreateOrUpdateEditionDto input);
        Task DeleteEdition(EntityDto input);
        Task<List<SubscribableEditionComboboxItemDto>> GetEditionComboboxItems(int? selectedEditionId = null, bool addAllItem = false, bool onlyFreeItems = false);
    }
}