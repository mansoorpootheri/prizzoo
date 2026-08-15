using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Geography
{
    public class GeographyAppService : ApplicationService, IGeographyAppService
    {
        private readonly IRepository<Country, int> _countryRepository;
        private readonly IRepository<State, int> _stateRepository;
        private readonly IRepository<District, int> _districtRepository;

        public GeographyAppService(
            IRepository<Country, int> countryRepository,
            IRepository<State, int> stateRepository,
            IRepository<District, int> districtRepository)
        {
            _countryRepository = countryRepository;
            _stateRepository = stateRepository;
            _districtRepository = districtRepository;
        }

        public async Task<List<ComboboxItemDto>> GetCountriesForCombobox()
        {
            var countries = await _countryRepository.GetAll().AsNoTracking()
            .OrderBy(x => x.CountryName)
            .ToListAsync();

            return countries.Select(x => new ComboboxItemDto
            {
                Value = x.Id.ToString(),
                DisplayText = x.CountryName
            }).ToList();

        }

        public async Task<List<ComboboxItemDto>> GetStatesByCountryForCombobox(int countryId)
        {
            var states = await _stateRepository.GetAll().AsNoTracking()
            .Where(x => x.CountryId == countryId)
            .OrderBy(x => x.StateName)
            .ToListAsync();

            return states.Select(x => new ComboboxItemDto
            {
                Value = x.Id.ToString(),
                DisplayText = x.StateName
            }).ToList();
        }

        public async Task<List<ComboboxItemDto>> GetDistrictsByStateForCombobox(int stateId)
        {
            var districts = await _districtRepository.GetAll().AsNoTracking()
            .Where(x => x.StateId == stateId)
            .OrderBy(x => x.DistrictName)
            .ToListAsync();

            return districts.Select(x => new ComboboxItemDto
            {
                Value = x.Id.ToString(),
                DisplayText = x.DistrictName
            }).ToList();
        }
    }
}