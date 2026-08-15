using EnterpriseBase.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Branches.Dto
{
    public class GetAllBranchesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public bool? IsHeadOffice { get; set; }
    }
}
