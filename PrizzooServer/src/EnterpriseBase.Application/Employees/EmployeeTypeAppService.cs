using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using EnterpriseBase.Authorization;
using EnterpriseBase.Branches;
using EnterpriseBase.Employees.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EnterpriseBase.Employees
{
    [AbpAuthorize(PermissionNames.Pages_Administration_EmployeeType)]
    public class EmployeeTypeAppService : EnterpriseBaseAppServiceBase, IEmployeeTypeAppService
    {
        private readonly IRepository<EmployeeType, int> _employeeTypeRepository;

        public EmployeeTypeAppService(IRepository<EmployeeType, int> employeeTypeRepository)
        {
            _employeeTypeRepository = employeeTypeRepository;
        }

        public async Task<PagedResultDto<GetEmployeeTypeForViewDto>> GetAll(GetAllEmployeeTypesInput input)
        {
            var filteredEmployeeTypes = _employeeTypeRepository.GetAll().AsNoTracking()
                         .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.Name.ToLower().Contains(input.Filter.ToLower()) || e.Description.ToLower().Contains(input.Filter.ToLower()))
                         .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive.Value);

            var pagedAndFilteredEmployeeTypes = filteredEmployeeTypes.OrderBy(input.Sorting ?? "id asc").PageBy(input);

            var employeeTypes = from o in pagedAndFilteredEmployeeTypes
                               select new
                               {
                                   o.Id,
                                   o.Name,
                                   o.Description,
                                   o.IsActive,
                                   o.TenantId,
                                   o.CreationTime
                               };
            var totalCount = await filteredEmployeeTypes.CountAsync();

            var dbList = await employeeTypes.ToListAsync();
            var results = new List<GetEmployeeTypeForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetEmployeeTypeForViewDto()
                {
                    EmployeeType = new EmployeeTypeDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Description = o.Description,
                        IsActive = o.IsActive,
                        TenantId = o.TenantId
                    }
                };
                results.Add(res);
            }

            return new PagedResultDto<GetEmployeeTypeForViewDto>(
                totalCount,
                results
            );
        }

        public async Task<GetEmployeeTypeForViewDto> GetEmployeeTypeForView(int id)
        {
            var employeeType = await _employeeTypeRepository.GetAsync(id);
            var output = new GetEmployeeTypeForViewDto { EmployeeType = ObjectMapper.Map<EmployeeTypeDto>(employeeType) };
            return output;
        }

        [AbpAuthorize(PermissionNames.Pages_Administration_EmployeeType_Edit)]
        public async Task<GetEmployeeTypeForEditOutput> GetEmployeeTypeForEdit(EntityDto input)
        {
            var employeeType = await _employeeTypeRepository.FirstOrDefaultAsync(input.Id);
            var output = new GetEmployeeTypeForEditOutput { EmployeeType = ObjectMapper.Map<CreateEmployeeTypeEditDto>(employeeType) };
            return output;
        }

        public async Task CreateOrEdit(CreateEmployeeTypeEditDto input)
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

        [AbpAuthorize(PermissionNames.Pages_Administration_EmployeeType_Create)]
        protected virtual async Task Create(CreateEmployeeTypeEditDto input)
        {
            var employeeType = ObjectMapper.Map<EmployeeType>(input);

            if (AbpSession.TenantId != null)
            {
                employeeType.TenantId = (int?)AbpSession.TenantId;
            }

            await _employeeTypeRepository.InsertAsync(employeeType);
        }
        
        [AbpAuthorize(PermissionNames.Pages_Administration_EmployeeType_Edit)]
        protected virtual async Task Update(CreateEmployeeTypeEditDto input)
        {
            var employeeType = await _employeeTypeRepository.FirstOrDefaultAsync((int)input.Id);
            input.TenantId = employeeType.TenantId;
            ObjectMapper.Map(input, employeeType);
        }

        [AbpAuthorize(PermissionNames.Pages_Administration_EmployeeType_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _employeeTypeRepository.DeleteAsync(input.Id);
        }

        public async Task<List<ComboboxItemDto>> GetEmployeeTypesForCombobox()
        {
            var list = await _employeeTypeRepository.GetAll().AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return list.Select(x => new ComboboxItemDto
            {
                Value       = x.Id.ToString(),
                DisplayText = x.Name
            }).ToList();
        }

    }
}