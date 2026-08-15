using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using EnterpriseBase.Authorization;
using EnterpriseBase.Branches.Dto;
using EnterpriseBase.Features;
using EnterpriseBase.Geography;
using EnterpriseBase.Roles.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EnterpriseBase.Branches
{

    public class BranchAppService : EnterpriseBaseAppServiceBase, IBranchAppService
    {
        private readonly IRepository<Branch, int> _branchRepository;
        private readonly IRepository<Country, int> _countryRepository;
        private readonly IRepository<State, int> _stateRepository;
        private readonly IRepository<District, int> _districtRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFeatureChecker _featureChecker;

        public BranchAppService(
            IRepository<Branch, int> branchRepository,
            IRepository<Country, int> countryRepository,
            IRepository<State, int> stateRepository,
            IRepository<District, int> districtRepository,
            IHttpContextAccessor httpContextAccessor,
            IFeatureChecker featureChecker)
        {
            _branchRepository = branchRepository;
            _countryRepository = countryRepository;
            _stateRepository = stateRepository;
            _districtRepository = districtRepository;
            _httpContextAccessor = httpContextAccessor;
            _featureChecker = featureChecker;
        }

        public async Task<PagedResultDto<GetBranchForViewDto>> GetAll(GetAllBranchesInput input)
        {
            var filteredBranches = _branchRepository.GetAll()
                         .Include(x => x.Country)
                         .Include(x => x.State)
                         .Include(x => x.District)
                         .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.BranchName.ToLower().Contains(input.Filter.ToLower()) || e.BranchCode.ToLower().Contains(input.Filter.ToLower()) || e.AddressLine1.ToLower().Contains(input.Filter.ToLower()) || e.AddressLine2.ToLower().Contains(input.Filter.ToLower()))
                         .WhereIf(input.IsHeadOffice.HasValue, x => x.IsHeadOffice == input.IsHeadOffice);

            var pagedAndFilteredBranches = filteredBranches.OrderBy(input.Sorting ?? "id asc").PageBy(input);

            var branches = from o in pagedAndFilteredBranches
                           join o1 in _districtRepository.GetAll() on o.DistrictId equals o1.Id into j1
                           from s1 in j1.DefaultIfEmpty()

                           join o2 in _stateRepository.GetAll() on o.StateId equals o2.Id into j2
                           from s2 in j2.DefaultIfEmpty()

                           join o3 in _countryRepository.GetAll() on o.CountryId equals o3.Id into j3
                           from s3 in j3.DefaultIfEmpty()

                           select new
                           {
                               o.Id,
                               o.BranchName,
                               o.BranchCode,
                               o.AddressLine1,
                               o.AddressLine2,
                               o.IsHeadOffice,
                               o.ContactPerson,
                               o.GstNumber,
                               o.MobileNumber,
                               o.PanNumber,
                               o.PhoneNumber,
                               o.Pincode,
                               o.Email,
                               o.TenantId,
                               o.CountryId,
                               o.StateId,
                               o.DistrictId,
                               o.CreationTime,
                               DistrictName = s1 == null || s1.DistrictName == null ? "" : s1.DistrictName.ToString(),
                               StateName = s2 == null || s2.StateName == null ? "" : s2.StateName,
                               CountryName = s3 == null || s3.CountryName == null ? "" : s3.CountryName
                           };
            var totalCount = await filteredBranches.CountAsync();

            var dbList = await branches.ToListAsync();
            var results = new List<GetBranchForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetBranchForViewDto()
                {
                    Branch = new BranchDto
                    {
                        Id = o.Id,
                        BranchName = o.BranchName,
                        BranchCode = o.BranchCode,
                        AddressLine1 = o.AddressLine1,
                        AddressLine2 = o.AddressLine2,
                        IsHeadOffice = o.IsHeadOffice,
                        ContactPerson = o.ContactPerson,
                        GstNumber = o.GstNumber,
                        MobileNumber = o.MobileNumber,
                        PanNumber = o.PanNumber,
                        PhoneNumber = o.PhoneNumber,
                        Pincode = o.Pincode,
                        Email = o.Email,
                        TenantId = o.TenantId,
                        CountryId = o.CountryId,
                        StateId = o.StateId,
                        DistrictId = o.DistrictId,
                        CreationTime = o.CreationTime
                    },
                    CountryName = o.CountryName,
                    StateName = o.StateName,
                    DistrictName = o.DistrictName

                };
                results.Add(res);
            }

            return new PagedResultDto<GetBranchForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetBranchForViewDto> GetBranchForView(int id)
        {
            var branch = await _branchRepository.GetAsync(id);

            var output = new GetBranchForViewDto { Branch = ObjectMapper.Map<BranchDto>(branch) };

            if (output.Branch.DistrictId != null)
            {
                var _lookupItem = await _districtRepository.FirstOrDefaultAsync((int)output.Branch.DistrictId);
                output.DistrictName = _lookupItem?.DistrictName?.ToString();
            }

            if (output.Branch.StateId != null)
            {
                var _lookupItem = await _stateRepository.FirstOrDefaultAsync((int)output.Branch.StateId);
                output.StateName = _lookupItem?.StateName?.ToString();
            }
            if (output.Branch.CountryId != null)
            {
                var _lookupItem = await _countryRepository.FirstOrDefaultAsync((int)output.Branch.CountryId);
                output.CountryName = _lookupItem?.CountryName?.ToString();
            }
            return output;
        }


        [AbpAuthorize(PermissionNames.Pages_Administration_Branch)]
        public async Task<GetBranchForEditOutput> GetBranchForEdit(EntityDto input)
        {
            var processingCenter = await _branchRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetBranchForEditOutput { Branch = ObjectMapper.Map<CreateBranchEditDto>(processingCenter) };

            if (output.Branch.DistrictId != null)
            {
                var _lookupItem = await _districtRepository.FirstOrDefaultAsync((int)output.Branch.DistrictId);
                output.DistrictName = _lookupItem?.DistrictName?.ToString();
            }

            if (output.Branch.StateId != null)
            {
                var _lookupItem = await _stateRepository.FirstOrDefaultAsync((int)output.Branch.StateId);
                output.StateName = _lookupItem?.StateName?.ToString();
            }
            if (output.Branch.CountryId != null)
            {
                var _lookupItem = await _countryRepository.FirstOrDefaultAsync((int)output.Branch.CountryId);
                output.CountryName = _lookupItem?.CountryName?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateBranchEditDto input)
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

        [AbpAuthorize(PermissionNames.Pages_Administration_Branch_Create)]
        protected virtual async Task Create(CreateBranchEditDto input)
        {
            if (AbpSession.TenantId.HasValue)
            {
                await CheckMaxBranchCountAsync(AbpSession.GetTenantId());
            }

            var branch = ObjectMapper.Map<Branch>(input);

            if (AbpSession.TenantId != null)
                branch.TenantId = (int?)AbpSession.TenantId;

            await _branchRepository.InsertAsync(branch);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        [AbpAuthorize(PermissionNames.Pages_Administration_Branch_Edit)]
        protected virtual async Task Update(CreateBranchEditDto input)
        {
            var branch = await _branchRepository.FirstOrDefaultAsync((int)input.Id);
            input.TenantId = branch.TenantId;
            ObjectMapper.Map(input, branch);
        }

        [AbpAuthorize(PermissionNames.Pages_Administration_Branch_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _branchRepository.DeleteAsync(input.Id);
        }

        /// <summary>
        /// this will return only branches which are assigned to users
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComboboxItemDto>> GetBranchesForCombobox()
        {
            var branchIdsClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("BranchIds")?.Value;
            var query = _branchRepository.GetAll();

            if (!string.IsNullOrEmpty(branchIdsClaim))
            {
                var branchIds = branchIdsClaim.Split(',').Select(int.Parse).ToList();
                query = query.Where(b => branchIds.Contains(b.Id));
            }

            var branches = await query.OrderBy(x => x.BranchName).ToListAsync();

            return branches.Select(x => new ComboboxItemDto
            {
                Value = x.Id.ToString(),
                DisplayText = x.BranchName
            }).ToList();
        }
        /// <summary>
        /// Use this for returning all branches 
        /// </summary>
        /// <returns></returns>
        public async Task<ListResultDto<BranchDto>> GetAllBranches()
        {

            var query = _branchRepository.GetAll();

            var branches = await query.ToListAsync();
            return new ListResultDto<BranchDto>(ObjectMapper.Map<List<BranchDto>>(branches));
        }

        private async Task CheckMaxBranchCountAsync(int tenantId)
        {
            var maxBranchCount = (await _featureChecker.GetValueAsync(tenantId, AppFeatures.MaxBranchCount)).To<int>();
            if (maxBranchCount <= 0)
                return;

            var currentBranchCount = await _branchRepository.CountAsync();
            if (currentBranchCount >= maxBranchCount)
            {
                throw new UserFriendlyException(L("MaximumBranchCount_Error_Message"), L("MaximumBranchCount_Error_Detail", maxBranchCount));
            }
        }
    }
}