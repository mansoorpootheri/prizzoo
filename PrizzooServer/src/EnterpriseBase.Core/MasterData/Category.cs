using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.MasterData
{
    // Shared public catalog data, not tenant-scoped - see Store.cs for why.
    [Table("Categories")]
    public class Category : FullAuditedEntity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(30)]
        public string Code { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
