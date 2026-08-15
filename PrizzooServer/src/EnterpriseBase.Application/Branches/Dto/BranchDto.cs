using Abp.Application.Services.Dto;
using System;

namespace EnterpriseBase.Branches.Dto
{
    public class BranchDto : EntityDto<int>
    {
        public int? TenantId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public int? DistrictId { get; set; }
        
        public int? StateId { get; set; }
        
        public int? CountryId { get; set; }
        
        public string Pincode { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public string GstNumber { get; set; }
        public string PanNumber { get; set; }
        public string ContactPerson { get; set; }
        public bool IsHeadOffice { get; set; }
        public DateTime CreationTime { get; set; }
    }
}