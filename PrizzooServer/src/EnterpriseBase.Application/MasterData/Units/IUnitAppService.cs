using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Application.MasterData.Units.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.MasterData.Units
{
    public interface IUnitAppService : IApplicationService
    {
        Task<List<UnitDto>> GetAllAsync();
        Task CreateOrEditAsync(CreateEditUnitDto input);
        Task DeleteAsync(EntityDto<Guid> input);
        Task<List<ComboboxItemDto>> GetForComboboxAsync();
    }
}
