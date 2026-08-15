using Abp.Application.Services.Dto;

namespace EnterpriseBase.Branches.Dto
{
    public class PagedBranchResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public bool? IsHeadOffice { get; set; }
    }
}