using Abp.Application.Services.Dto;

namespace EnterpriseBase.Employees.Dto
{
    public class PagedEmployeeResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? Status { get; set; }
        public int? EmployeeTypeId { get; set; }
        public int? BranchId { get; set; }
    }
}