using EnterpriseBase.Dto;

namespace EnterpriseBase.Employees.Dto
{
    public class GetAllEmployeesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public bool? Status { get; set; }
        public int? EmployeeTypeId { get; set; }
        public int? BranchId { get; set; }
    }
}