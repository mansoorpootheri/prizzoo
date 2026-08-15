using Abp.Application.Services.Dto;

namespace EnterpriseBase.Employees.Dto
{
    public class EmployeeTypeDto : EntityDto<int>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}