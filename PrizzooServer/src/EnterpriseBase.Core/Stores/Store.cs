using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using EnterpriseBase.MasterData;
using EnterpriseBase.Storage;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Stores
{
    /// <summary>
    /// A physical, walk-in retail store shown in the public price comparison index.
    ///
    /// Deliberately NOT IMustHaveTenant: stores are the public inventory of the
    /// platform itself, visible to every shopper regardless of who manages them.
    /// There is no per-store owner login (that self-service model was built and
    /// then removed) - a single Admin identity (phone+OTP) manages every store
    /// directly, so there's nothing here to scope by owner.
    ///
    /// See DOCS/PRIZZOO_MIGRATION_NOTES.md for the reasoning behind this decision.
    /// </summary>
    [Table("Stores")]
    public class Store : FullAuditedEntity<Guid>
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Unused in Phase 1 (single-tenant public catalog). Reserved for Phase 3
        /// white-label/franchise mode where a retailer tenant owns their store listings.
        /// Do NOT add IMayHaveTenant yet — ABP's tenant filter would silently hide
        /// host-created stores from tenant queries. Keep it as a plain nullable FK.
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// The retail chain this location belongs to (e.g. all myG stores share one ChainId).
        /// Null for independent shops. Unused in Phase 1 — set during Phase 3 retailer onboarding.
        /// </summary>
        public Guid? ChainId { get; set; }
        [ForeignKey("ChainId")]
        public virtual StoreChain Chain { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        /// <summary>
        /// Denormalized display copy of Location.City.Name, kept in sync by
        /// StoreAppService whenever LocationId is set - existing filtering/
        /// display code (PagedStoreRequestDto.City, StoreDto.City) keeps
        /// working unchanged. LocationId is the source of truth going
        /// forward; this string is a convenience read, not authoritative.
        /// </summary>
        [MaxLength(100)]
        public string City { get; set; }

        /// <summary>
        /// The specific locality within City, e.g. "Feroke" within
        /// "Calicut". Nullable so existing free-text-only stores (created
        /// before this field existed) remain valid.
        /// </summary>
        public Guid? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }

        [Phone]
        [MaxLength(30)]
        public string Phone { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [MaxLength(500)]
        public string OpeningHours { get; set; }

        /// <summary>
        /// Free-text category tags, e.g. "mobiles,electronics" or "pharmacy".
        /// Kept simple for MVP rather than a full category-mapping table.
        /// </summary>
        [MaxLength(200)]
        public string CategoryTags { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public Guid? ImageId { get; set; }
        [ForeignKey("ImageId")]
        public virtual BinaryObject Image { get; set; }
    }
}
