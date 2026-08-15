using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using EnterpriseBase.Storage;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.MasterData
{
    /// <summary>
    /// Catalog metadata only - deliberately has no price field. The same product
    /// has many different prices across many different stores; see
    /// EnterpriseBase.Pricing.Price for where that actually lives, and
    /// DOCS/PRIZZOO_MIGRATION_NOTES.md for why this was split from the original
    /// FieldSale Product (which had a single tenant-owned SellingPrice).
    ///
    /// Shared public catalog data, not tenant-scoped - see Store.cs for why.
    /// </summary>
    [Table("Products")]
    public class Product : FullAuditedEntity<Guid>
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Barcode { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public Guid? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public Guid? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; }

        public Guid? ImageId { get; set; }
        [ForeignKey("ImageId")]
        public virtual BinaryObject Image { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
