using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.MasterData
{
    // Shared public catalog data, not tenant-scoped - see Store.cs for why.
    [Table("Units")]
    public class Unit : FullAuditedEntity<Guid>
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; }

        public int DecimalPlaces { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
