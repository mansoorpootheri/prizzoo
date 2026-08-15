using Abp.Domain.Entities.Auditing;
using EnterpriseBase.Storage;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Stores
{
    /// <summary>
    /// A retail brand/chain that operates multiple Store locations (e.g. "myG", "Reliance Digital").
    /// 
    /// Phase 1: data model only — no UI, no service. Every Store.ChainId is null.
    /// Phase 3: when a retailer self-onboards as a tenant, their TenantId is set here
    /// and all their Store rows are linked via ChainId — giving them a single login
    /// to manage all locations.
    /// </summary>
    [Table("StoreChains")]
    public class StoreChain : FullAuditedEntity<Guid>
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Website { get; set; }

        /// <summary>
        /// Phase 3: the Prizzoo tenant that owns and self-manages this chain's listings.
        /// Nullable — chains added by ops staff in Phase 1/2 have no tenant yet.
        /// Plain int?, NOT IMayHaveTenant, for the same reason as Store.TenantId.
        /// </summary>
        public int? TenantId { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public Guid? LogoId { get; set; }
        [ForeignKey("LogoId")]
        public virtual BinaryObject Logo { get; set; }
    }
}
