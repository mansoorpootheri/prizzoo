using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using EnterpriseBase.Geography;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Branches
{
    [Table("Branches")]
    public class Branch : FullAuditedEntity<int>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        [MaxLength(150)]
        public string BranchName { get; set; }

        [MaxLength(20)]
        public string BranchCode { get; set; }

        [MaxLength(200)]
        public string AddressLine1 { get; set; }

        [MaxLength(200)]
        public string AddressLine2 { get; set; }

        public int? DistrictId { get; set; }
        [ForeignKey("DistrictId")]
        public virtual District District { get; set; }

        public int? StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual State State { get; set; }

        public int? CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

        [MaxLength(10)]
        public string Pincode { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(20)]
        public string MobileNumber { get; set; }

        [MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string GstNumber { get; set; }

        [MaxLength(15)]
        public string PanNumber { get; set; }

        [MaxLength(100)]
        public string ContactPerson { get; set; }

        public bool IsHeadOffice { get; set; }
    }
}
