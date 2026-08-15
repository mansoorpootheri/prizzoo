using Abp.Application.Services.Dto;

namespace EnterpriseBase.Employees.Dto
{
    public class PagedEmployeeTypeResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(Sorting))
            {
                Sorting = "Name";
            }

            Keyword = Keyword?.Trim();
        }
    }
}