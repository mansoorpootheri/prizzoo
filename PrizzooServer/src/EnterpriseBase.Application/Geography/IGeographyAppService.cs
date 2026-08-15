using Abp.Application.Services.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Geography
{
    public interface IGeographyAppService
    {
        Task<List<ComboboxItemDto>> GetCountriesForCombobox();
        Task<List<ComboboxItemDto>> GetStatesByCountryForCombobox(int countryId);
        Task<List<ComboboxItemDto>> GetDistrictsByStateForCombobox(int stateId);
    }
}