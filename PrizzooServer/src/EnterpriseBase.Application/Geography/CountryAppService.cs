using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using EnterpriseBase.Authorization;
using EnterpriseBase.Geography.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EnterpriseBase.Geography
{
    [AbpAuthorize(PermissionNames.Pages_Geography_Countries)]
    public class CountryAppService : EnterpriseBaseAppServiceBase, ICountryAppService
    {
        private readonly IRepository<Country, int> _countryRepository;

        public CountryAppService(IRepository<Country, int> countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public async Task<PagedResultDto<GetCountryForViewDto>> GetAll(GetAllCountriesInput input)
        {
            var filteredCountries = _countryRepository.GetAll()
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.CountryName.Contains(input.Filter) || e.IsoCode.Contains(input.Filter));

            var pagedAndFilteredCountries = filteredCountries.OrderBy(input.Sorting ?? "id asc").PageBy(input);

            var countries = from o in pagedAndFilteredCountries
                           select new
                           {
                               o.Id,
                               o.CountryName,
                               o.IsoCode,
                               o.PhoneCode
                           };

            var totalCount = await filteredCountries.CountAsync();
            var dbList = await countries.ToListAsync();
            var results = new List<GetCountryForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetCountryForViewDto()
                {
                    Country = new CountryDto
                    {
                        Id = o.Id,
                        CountryName = o.CountryName,
                        IsoCode = o.IsoCode,
                        PhoneCode = o.PhoneCode
                    }
                };
                results.Add(res);
            }

            return new PagedResultDto<GetCountryForViewDto>(totalCount, results);
        }

        public async Task<GetCountryForViewDto> GetCountryForView(int id)
        {
            var country = await _countryRepository.GetAsync(id);
            var output = new GetCountryForViewDto { Country = ObjectMapper.Map<CountryDto>(country) };
            return output;
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_Countries_Edit)]
        public async Task<GetCountryForEditOutput> GetCountryForEdit(EntityDto input)
        {
            var country = await _countryRepository.FirstOrDefaultAsync(input.Id);
            var output = new GetCountryForEditOutput { Country = ObjectMapper.Map<CreateCountryEditDto>(country) };
            return output;
        }

        public async Task CreateOrEdit(CreateCountryEditDto input)
        {
            if (input.Id == 0)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_Countries_Create)]
        protected virtual async Task Create(CreateCountryEditDto input)
        {
            var country = ObjectMapper.Map<Country>(input);
            await _countryRepository.InsertAsync(country);
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_Countries_Edit)]
        protected virtual async Task Update(CreateCountryEditDto input)
        {
            var country = await _countryRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, country);
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_Countries_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _countryRepository.DeleteAsync(input.Id);
        }
    }
}