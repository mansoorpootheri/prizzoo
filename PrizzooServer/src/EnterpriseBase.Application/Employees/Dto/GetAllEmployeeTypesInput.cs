using EnterpriseBase.Dto;

namespace EnterpriseBase.Employees.Dto
{
    public class GetAllEmployeeTypesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public bool? IsActive { get; set; }
    }
}