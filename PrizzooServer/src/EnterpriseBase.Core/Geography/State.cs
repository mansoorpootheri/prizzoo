using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Geography
{
    [Table("States")]
    public class State : FullAuditedEntity<int>
    {

        [Required]
        [MaxLength(100)]
        public string StateName { get; set; }

        [MaxLength(10)]
        public string StateCode { get; set; }

        [Required]
        public int CountryId { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }
    }
}