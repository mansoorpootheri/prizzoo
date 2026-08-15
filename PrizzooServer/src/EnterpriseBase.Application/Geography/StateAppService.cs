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
    [AbpAuthorize(PermissionNames.Pages_Geography_States)]
    public class StateAppService : EnterpriseBaseAppServiceBase, IStateAppService
    {
        private readonly IRepository<State, int> _stateRepository;
        private readonly IRepository<Country, int> _countryRepository;

        public StateAppService(IRepository<State, int> stateRepository, IRepository<Country, int> countryRepository)
        {
            _stateRepository = stateRepository;
            _countryRepository = countryRepository;
        }

        public async Task<PagedResultDto<GetStateForViewDto>> GetAll(GetAllStatesInput input)
        {
            var filteredStates = _stateRepository.GetAll()
                .Include(x => x.Country)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.StateName.Contains(input.Filter) || e.StateCode.Contains(input.Filter))
                .WhereIf(input.CountryId.HasValue, x => x.CountryId == input.CountryId);

            var pagedAndFilteredStates = filteredStates.OrderBy(input.Sorting ?? "id asc").PageBy(input);

            var states = from o in pagedAndFilteredStates
                        join o1 in _countryRepository.GetAll() on o.CountryId equals o1.Id into j1
                        from s1 in j1.DefaultIfEmpty()
                        select new
                        {
                            o.Id,
                            o.StateName,
                            o.StateCode,
                            o.CountryId,
                            CountryName = s1 == null || s1.CountryName == null ? "" : s1.CountryName
                        };

            var totalCount = await filteredStates.CountAsync();
            var dbList = await states.ToListAsync();
            var results = new List<GetStateForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetStateForViewDto()
                {
                    State = new StateDto
                    {
                        Id = o.Id,
                        StateName = o.StateName,
                        StateCode = o.StateCode,
                        CountryId = o.CountryId
                    },
                    CountryName = o.CountryName
                };
                results.Add(res);
            }

            return new PagedResultDto<GetStateForViewDto>(totalCount, results);
        }

        public async Task<GetStateForViewDto> GetStateForView(int id)
        {
            var state = await _stateRepository.GetAsync(id);
            var output = new GetStateForViewDto { State = ObjectMapper.Map<StateDto>(state) };

            if (output.State.CountryId != 0)
            {
                var country = await _countryRepository.FirstOrDefaultAsync(output.State.CountryId);
                output.CountryName = country?.CountryName;
            }

            return output;
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_States_Edit)]
        public async Task<GetStateForEditOutput> GetStateForEdit(EntityDto input)
        {
            var state = await _stateRepository.FirstOrDefaultAsync(input.Id);
            var output = new GetStateForEditOutput { State = ObjectMapper.Map<CreateStateEditDto>(state) };

            if (output.State.CountryId != 0)
            {
                var country = await _countryRepository.FirstOrDefaultAsync(output.State.CountryId);
                output.CountryName = country?.CountryName;
            }

            return output;
        }

        public async Task CreateOrEdit(CreateStateEditDto input)
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

        [AbpAuthorize(PermissionNames.Pages_Geography_States_Create)]
        protected virtual async Task Create(CreateStateEditDto input)
        {
            var state = ObjectMapper.Map<State>(input);
            await _stateRepository.InsertAsync(state);
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_States_Edit)]
        protected virtual async Task Update(CreateStateEditDto input)
        {
            var state = await _stateRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, state);
        }

        [AbpAuthorize(PermissionNames.Pages_Geography_States_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _stateRepository.DeleteAsync(input.Id);
        }

        public async Task<ListResultDto<StateDto>> GetStatesByCountryAsync(int countryId)
        {
            var states = await _stateRepository.GetAll().AsNoTracking()
                .Include(s => s.Country)
                .Where(s => s.CountryId == countryId)
                .ToListAsync();
            return new ListResultDto<StateDto>(ObjectMapper.Map<StateDto[]>(states));
        }
    }
}