using EnterpriseBase.Dto;

namespace EnterpriseBase.Geography.Dto
{
    public class GetAllCountriesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}