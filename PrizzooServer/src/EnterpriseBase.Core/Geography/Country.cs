using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Geography
{
    [Table("Countries")]
    public class Country : FullAuditedEntity<int>
    {
        [Required]
        [MaxLength(100)]
        public string CountryName { get; set; }

        [MaxLength(10)]
        public string IsoCode { get; set; }

        [MaxLength(10)]
        public string PhoneCode { get; set; }
    }
}