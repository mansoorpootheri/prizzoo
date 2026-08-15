using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Branches.Dto
{
    public class CreateBranchEditDto : EntityDto<int>
    {
        public int? TenantId { get; set; }

        [Required]
        [StringLength(150)]
        public string BranchName { get; set; }

        [StringLength(30)]
        public string BranchCode { get; set; }

        [StringLength(200)]
        public string AddressLine1 { get; set; }

        [StringLength(200)]
        public string AddressLine2 { get; set; }

        public int? DistrictId { get; set; }
        public int? StateId { get; set; }
        public int? CountryId { get; set; }

        [StringLength(10)]
        public string Pincode { get; set; }

        [StringLength(30)]
        public string PhoneNumber { get; set; }

        [StringLength(30)]
        public string MobileNumber { get; set; }

        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(30)]
        public string GstNumber { get; set; }

        [StringLength(15)]
        public string PanNumber { get; set; }

        [StringLength(100)]
        public string ContactPerson { get; set; }

        public bool IsHeadOffice { get; set; }
    }
}