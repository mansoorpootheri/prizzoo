using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Application.MasterData.Categories.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.MasterData.Categories
{
    public interface ICategoryAppService : IApplicationService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task CreateOrEditAsync(CreateEditCategoryDto input);
        Task DeleteAsync(EntityDto<Guid> input);
        Task<List<ComboboxItemDto>> GetForComboboxAsync();
    }
}
