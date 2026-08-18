using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using EnterpriseBase.Geography;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.MasterData
{
    /// <summary>
    /// A specific locality/area within a District, e.g. "Feroke" within the
    /// "Kozhikode" district. Shared public catalog data, not tenant-scoped -
    /// see Store.cs for why. Country/State are fixed constants for MVP
    /// (India/Kerala, seeded by DefaultGeographyCreator along with all of
    /// Kerala's districts) - District is the level that plays the "city"
    /// role in the store-creation form, and Location narrows within it.
    /// </summary>
    [Table("Locations")]
    public class Location : FullAuditedEntity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int DistrictId { get; set; }
        [ForeignKey("DistrictId")]
        public virtual District District { get; set; }

        /// <summary>
        /// The locality's approximate centroid. Nullable - existing
        /// locations predate this field and older/free-text entries may
        /// never get precise coordinates. Once set, a shopper's live GPS
        /// position can be matched against these to auto-detect which
        /// Location they're in, the same way Store.Latitude/Longitude
        /// already works for exact store pins.
        /// </summary>
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
