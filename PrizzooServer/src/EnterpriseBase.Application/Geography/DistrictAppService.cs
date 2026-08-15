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
    [AbpAuthorize(PermissionNames.Pages_Geography_Districts)]
    public class DistrictAppService : EnterpriseBaseAppServiceBase, IDistrictAppService
    {
        private readonly IRepository<District, int> _districtRepository;
        private readonly IRepository<State, int> _stateRepository;
        private readonly IRepository<Country, int> _countryRepository;

        public DistrictAppService(IRepository<District, int> districtRepository, IRepository<State, int> stateRepository, IRepository<Country, int> countryRepository)
        {
            _districtRepository = districtRepository;
            _stateRepository = stateRepository;
            _countryRepository = countryRepository;
        }

        public async Task<PagedResultDto<GetDistrictForViewDto>> GetAll(GetAllDistrictsInput input)
        {
            var filteredDistricts = _districtRepository.GetAll()
                .Include(x => x.State)
                .ThenInclude(x => x.Country)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.DistrictName.Contains(input.Filter))
                .WhereIf(input.StateId.HasValue, x => x.StateId == input.StateId);

            var pagedAndFilteredDistricts = filteredDistricts.OrderBy(input.Sorting ?? "id asc").PageBy(input);

            var districts = from o in pagedAndFilteredDistricts
                           join o1 in _stateRepository.GetAll() on o.StateId equals o1.Id into j1
                           from s1 in j1.DefaultIfEmpty()
                           join o2 in _countryRepository.GetAll() on s1.CountryId equals o2.Id into j2
                           from s2 in j2.DefaultIfEmpty()
                           select new
                           {
                               o.Id,
                               o.DistrictName,
                               o.StateId,
                               StateName = s1 == null || s1.StateName == null ? "" : s1.StateName,
                               CountryName = s2 == null || s2.CountryName == null ? "" : s2.CountryName
                           };

            var totalCount = await filteredDistricts.CountAsync();
            var dbList = await districts.ToListAsync();
            var results = new List<GetDistrictForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetDistrictForViewDto()
                {
                    District = new DistrictDto
                    {
                        Id = o.Id,
                        DistrictName = o.DistrictName,
                        StateId = o.StateId
                    },
                    StateName = o.StateName,
                    CountryName = o.CountryName
                };
                results.Add(res);
            }

            return new PagedResultDto<GetDistrictForViewDto>(totalCount, results);
        }

        public async Task<GetDistrictForViewDto> GetDistrictForView(int id)
        {
            var district = await _districtRepository.GetAsync(id);
            var output = new GetDistrictForViewDto { District = ObjectMapper.Map<DistrictDto>(district) };

            if (output.District.StateId != 0)
            {
                var state = await _stateRepository.FirstOrDefaultAsync(output.District.StateId);
                output.StateName = state?.StateName;

                if (state?.CountryId != null)
                {
                    var country = await _countryRepository.FirstOrDefaultAsync(state.CountryId);
                    output.CountryName = country?.CountryName;
                }
            }

            return output;
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_Districts_Edit)]
        public async Task<GetDistrictForEditOutput> GetDistrictForEdit(EntityDto input)
        {
            var district = await _districtRepository.FirstOrDefaultAsync(input.Id);
            var output = new GetDistrictForEditOutput { District = ObjectMapper.Map<CreateDistrictEditDto>(district) };

            if (output.District.StateId != 0)
            {
                var state = await _stateRepository.FirstOrDefaultAsync(output.District.StateId);
                output.StateName = state?.StateName;

                if (state?.CountryId != null)
                {
                    var country = await _countryRepository.FirstOrDefaultAsync(state.CountryId);
                    output.CountryName = country?.CountryName;
                }
            }

            return output;
        }

        public async Task CreateOrEdit(CreateDistrictEditDto input)
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

        [AbpAuthorize(PermissionNames.Pages_Geography_Districts_Create)]
        protected virtual async Task Create(CreateDistrictEditDto input)
        {
            var district = ObjectMapper.Map<District>(input);
            await _districtRepository.InsertAsync(district);
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_Districts_Edit)]
        protected virtual async Task Update(CreateDistrictEditDto input)
        {
            var district = await _districtRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, district);
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_Districts_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _districtRepository.DeleteAsync(input.Id);
        }

        public async Task<ListResultDto<DistrictDto>> GetDistrictsByStateAsync(int stateId)
        {
            var districts = await _districtRepository.GetAll()
                .Include(d => d.State)
                .ThenInclude(s => s.Country)
                .Where(d => d.StateId == stateId)
                .ToListAsync();
            return new ListResultDto<DistrictDto>(ObjectMapper.Map<DistrictDto[]>(districts));
        }
    }
}