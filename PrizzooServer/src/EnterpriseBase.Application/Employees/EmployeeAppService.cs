using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using EnterpriseBase.Authorization;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Branches;
using EnterpriseBase.Employees.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EnterpriseBase.Employees
{
    [AbpAuthorize(PermissionNames.Pages_Administration_Employee)]
    public class EmployeeAppService : EnterpriseBaseAppServiceBase, IEmployeeAppService
    {
        private readonly IRepository<Employee, long> _employeeRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<EmployeeType, int> _employeeTypeRepository;
        private readonly IBranchAppService _branchManager;
        public EmployeeAppService(IRepository<Employee, long> employeeRepository, IRepository<User, long> userRepository,
            IRepository<EmployeeType, int> employeeTypeRepository, IBranchAppService branchManager)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _employeeTypeRepository = employeeTypeRepository;
            _branchManager = branchManager;
        }
        public async Task<PagedResultDto<GetEmployeeForViewDto>> GetAll(GetAllEmployeesInput input)
        {
            using (CurrentUnitOfWork.DisableFilter("MustHaveBranch"))
            {
            var filteredEmployees = _employeeRepository.GetAll()
                         .Include(x => x.EmployeeType)
                         .Include(x => x.Branch)
                         .Include(x => x.User)
                         .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.Name.ToLower().Contains(input.Filter.ToLower()) || e.Code.ToLower().Contains(input.Filter.ToLower()) || (e.EmployeeNumber != null && e.EmployeeNumber.ToLower().Contains(input.Filter.ToLower())))
                         .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
                         .WhereIf(input.EmployeeTypeId.HasValue, x => x.EmployeeTypeId == input.EmployeeTypeId.Value)
                         .WhereIf(input.BranchId.HasValue, x => x.BranchId == input.BranchId.Value);

            var pagedAndFilteredEmployees = filteredEmployees.OrderBy(input.Sorting ?? "id asc").PageBy(input);

            var employees = from o in pagedAndFilteredEmployees
                           join o1 in _employeeTypeRepository.GetAll() on o.EmployeeTypeId equals o1.Id into j1
                           from s1 in j1.DefaultIfEmpty()
                           join o3 in _userRepository.GetAll() on o.UserId equals o3.Id into j3
                           from s3 in j3.DefaultIfEmpty()

                           select new
                           {
                               o.Id,
                               o.Name,
                               o.Code,
                               o.MobileNo,
                               o.Email,
                               o.Gender,
                               o.EmployeeTypeId,
                               o.Status,
                               o.BranchId,
                               o.EmployeeNumber,
                               o.DateOfBirth,
                               o.JoiningDate,
                               o.Designation,
                               o.Address,
                               o.City,
                               o.PostalCode,
                               o.AlternatePhone,
                               o.EmergencyContactName,
                               o.EmergencyContactPhone,
                               o.EmergencyContactRelation,
                               o.BasicSalary,
                               o.BankAccountNumber,
                               o.BankName,
                               o.PANNumber,
                               o.AadharNumber,
                               o.Notes,
                               o.UserId,
                               o.CreationTime,
                               EmployeeTypeName = s1 == null || s1.Name == null ? "" : s1.Name,
                               BranchName = o.Branch != null ? o.Branch.BranchName : "",
                               UserName = s3 == null || s3.UserName == null ? "" : s3.UserName
                           };
            var totalCount = await filteredEmployees.CountAsync();

            var dbList = await employees.ToListAsync();
            var results = new List<GetEmployeeForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetEmployeeForViewDto()
                {
                    Employee = new EmployeeDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Code = o.Code,
                        MobileNo = o.MobileNo,
                        Email = o.Email,
                        Gender = o.Gender,
                        EmployeeTypeId = o.EmployeeTypeId,
                        Status = o.Status,
                        BranchId = o.BranchId,
                        EmployeeNumber = o.EmployeeNumber,
                        DateOfBirth = o.DateOfBirth,
                        JoiningDate = o.JoiningDate,
                        Designation = o.Designation,
                        Address = o.Address,
                        City = o.City,
                        PostalCode = o.PostalCode,
                        AlternatePhone = o.AlternatePhone,
                        EmergencyContactName = o.EmergencyContactName,
                        EmergencyContactPhone = o.EmergencyContactPhone,
                        EmergencyContactRelation = o.EmergencyContactRelation,
                        BasicSalary = o.BasicSalary,
                        BankAccountNumber = o.BankAccountNumber,
                        BankName = o.BankName,
                        PANNumber = o.PANNumber,
                        AadharNumber = o.AadharNumber,
                        Notes = o.Notes,
                        UserId = o.UserId,
                        CreationTime = o.CreationTime
                    },
                    EmployeeTypeName = o.EmployeeTypeName,
                    BranchName = o.BranchName,
                    UserName = o.UserName
                };
                results.Add(res);
            }

            return new PagedResultDto<GetEmployeeForViewDto>(
                totalCount,
                results
            );
            }
        }

        public async Task<GetEmployeeForViewDto> GetEmployeeForView(long id)
        {
            var employee = await _employeeRepository.GetAsync(id);

            var output = new GetEmployeeForViewDto { Employee = ObjectMapper.Map<EmployeeDto>(employee) };

            if (output.Employee.EmployeeTypeId != null)
            {
                var _lookupItem = await _employeeTypeRepository.FirstOrDefaultAsync((int)output.Employee.EmployeeTypeId);
                output.EmployeeTypeName = _lookupItem?.Name?.ToString();
            }

            if (output.Employee.BranchId != null)
            {
                var branches = await _branchManager.GetBranchesForCombobox();
                var branch = branches.FirstOrDefault(b => b.Value == output.Employee.BranchId.ToString());
                output.BranchName = branch?.DisplayText;
            }
            if (output.Employee.UserId != null)
            {
                var _lookupItem = await _userRepository.FirstOrDefaultAsync((long)output.Employee.UserId);
                output.UserName = _lookupItem?.UserName?.ToString();
            }
            return output;
        }

        [AbpAuthorize(PermissionNames.Pages_Administration_Employee_Edit)]
        public async Task<GetEmployeeForEditOutput> GetEmployeeForEdit(EntityDto<long> input)
        {
            var employee = await _employeeRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetEmployeeForEditOutput { Employee = ObjectMapper.Map<CreateEmployeeEditDto>(employee) };

            if (output.Employee.EmployeeTypeId != null)
            {
                var _lookupItem = await _employeeTypeRepository.FirstOrDefaultAsync((int)output.Employee.EmployeeTypeId);
                output.EmployeeTypeName = _lookupItem?.Name?.ToString();
            }

            if (output.Employee.BranchId != null)
            {
                var branches = await _branchManager.GetBranchesForCombobox();
                var branch = branches.FirstOrDefault(b => b.Value == output.Employee.BranchId.ToString());
                output.BranchName = branch?.DisplayText;
            }
            if (output.Employee.UserId != null)
            {
                var _lookupItem = await _userRepository.FirstOrDefaultAsync((long)output.Employee.UserId);
                output.UserName = _lookupItem?.UserName?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateEmployeeEditDto input)
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

        [AbpAuthorize(PermissionNames.Pages_Administration_Employee_Create)]
        protected virtual async Task Create(CreateEmployeeEditDto input)
        {
            // Code is optional on the DTO (and this method already only duplicate-checks it
            // "if provided"), but the Employees.Code column is NOT NULL — auto-generate one
            // when the caller (e.g. the mobile app's minimal Add Employee form) doesn't supply
            // it, mirroring PartyAppService.GenerateNextPartyCodeAsync's pattern.
            if (string.IsNullOrWhiteSpace(input.Code))
            {
                input.Code = await GenerateNextEmployeeCodeAsync();
            }
            else
            {
                var existingEmployeeWithCode = await _employeeRepository.FirstOrDefaultAsync(e => e.Code == input.Code);
                if (existingEmployeeWithCode != null)
                    throw new UserFriendlyException($"Employee with code '{input.Code}' already exists.");
            }

            if (input.UserId.HasValue)
            {
                var existingEmployee = await _employeeRepository.FirstOrDefaultAsync(e => e.UserId == input.UserId.Value);
                if (existingEmployee != null)
                    throw new UserFriendlyException(L("UserAlreadyLinkedToEmployee"));
            }

            var employee = ObjectMapper.Map<Employee>(input);
            // input.BranchId is nullable (DTO), but Employee.BranchId is a non-nullable int
            // (IMustHaveBranch) — without this, ObjectMapper.Map throws when a caller (like
            // the mobile app's minimal Add Employee form) omits BranchId. Mirrors the same
            // fallback already applied to employeeAccount.BranchId above.
            employee.BranchId = input.BranchId ?? GetCurrentBranchId();
            if (AbpSession.TenantId != null)
                employee.TenantId = (int?)AbpSession.TenantId;

            await _employeeRepository.InsertAsync(employee);
        }

        private async Task<string> GenerateNextEmployeeCodeAsync()
        {
            const string prefix = "EMP";
            var maxCode = await _employeeRepository.GetAll()
                .Where(e => e.Code.StartsWith(prefix + "-"))
                .Select(e => e.Code)
                .OrderByDescending(e => e)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (maxCode != null)
            {
                var numPart = maxCode.Substring(prefix.Length + 1);
                if (int.TryParse(numPart, out var parsed))
                    nextNumber = parsed + 1;
            }
            return $"{prefix}-{nextNumber:D5}";
        }

        [AbpAuthorize(PermissionNames.Pages_Administration_Employee_Edit)]
        protected virtual async Task Update(CreateEmployeeEditDto input)
        {
            if (!string.IsNullOrWhiteSpace(input.Code))
            {
                var existingEmployeeWithCode = await _employeeRepository.FirstOrDefaultAsync(e => e.Code == input.Code && e.Id != input.Id);
                if (existingEmployeeWithCode != null)
                    throw new UserFriendlyException($"Employee with code '{input.Code}' already exists.");
            }

            if (input.UserId.HasValue)
            {
                var existingEmployee = await _employeeRepository.FirstOrDefaultAsync(e => e.UserId == input.UserId.Value && e.Id != input.Id);
                if (existingEmployee != null)
                    throw new UserFriendlyException("User Already Linked To Employee");
            }

            var employee = await _employeeRepository.FirstOrDefaultAsync((long)input.Id);

            // Same optional-DTO/required-entity mismatch as BranchId below — a caller (like
            // the mobile app's minimal Edit Employee form) that omits Code shouldn't blank out
            // the employee's existing code (Employees.Code is NOT NULL) or its linked ledger
            // account's code; keep whatever's already on file.
            var effectiveCode = string.IsNullOrWhiteSpace(input.Code) ? employee.Code : input.Code;
            input.Code = effectiveCode;

            input.TenantId = employee.TenantId;
            var existingBranchId = employee.BranchId;
            ObjectMapper.Map(input, employee);
            // Same nullable-DTO/non-nullable-entity mismatch as Create above — a caller (like
            // the mobile app's minimal Edit Employee form) that omits BranchId shouldn't
            // silently move the employee to a different branch; keep the one already on file.
            employee.BranchId = input.BranchId ?? existingBranchId;
        }

        [AbpAuthorize(PermissionNames.Pages_Administration_Employee_Delete)]
        public async Task Delete(EntityDto<long> input)
        {
            var employee = await _employeeRepository.GetAsync(input.Id);

            await _employeeRepository.DeleteAsync(employee.Id);
        }

        [AbpAllowAnonymous]
        public async Task<List<ComboboxItemDto>> GetUsers()
        {
            var users = await _userRepository.GetAll().AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.UserName)
                .ToListAsync();

            return users.Select(x => new ComboboxItemDto
            {
                Value = x.Id.ToString(),
                DisplayText = x.UserName + " (" + x.Name + " " + x.Surname + ")"
            }).ToList();
        }

        public async Task<List<ComboboxItemDto>> GetEmployeeTypesForCombobox()
        {
            var employeeTypes = await _employeeTypeRepository.GetAll().AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return employeeTypes.Select(x => new ComboboxItemDto
            {
                Value = x.Id.ToString(),
                DisplayText = x.Name
            }).ToList();
        }
        public async Task<List<ComboboxItemDto>> GetBranchesForCombobox()
        {
            return await _branchManager.GetBranchesForCombobox();
        }
    }
}