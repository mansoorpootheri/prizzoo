using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Taxes.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Taxes
{
    public interface ITaxAppService : IApplicationService
    {
        Task<List<TaxDto>> GetAllAsync();
        Task CreateOrEditAsync(CreateEditTaxDto input);
        Task DeleteAsync(EntityDto<Guid> input);
        Task<List<ComboboxItemDto>> GetForComboboxAsync();
    }
}
