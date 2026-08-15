using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Branches.Dto
{
    public class GetBranchForEditOutput
    {
        public CreateBranchEditDto Branch { get; set; }
        public string DistrictName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
    }
}
