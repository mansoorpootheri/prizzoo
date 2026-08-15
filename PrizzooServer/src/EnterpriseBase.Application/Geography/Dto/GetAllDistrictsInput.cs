using EnterpriseBase.Dto;

namespace EnterpriseBase.Geography.Dto
{
    public class GetAllDistrictsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public int? StateId { get; set; }
    }
}