using EnterpriseBase.Dto;

namespace EnterpriseBase.Geography.Dto
{
    public class GetAllStatesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public int? CountryId { get; set; }
    }
}