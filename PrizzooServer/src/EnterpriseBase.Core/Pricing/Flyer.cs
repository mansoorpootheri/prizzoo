using Abp.Domain.Entities.Auditing;
using EnterpriseBase.Storage;
using EnterpriseBase.Stores;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Pricing
{
    /// <summary>
    /// A flyer photo an admin uploaded for a store, together with the
    /// hand-typed item list submitted alongside it (see FlyerAppService.
    /// CreateFlyerForStoreAsync) - each item becomes a real, already-
    /// Approved Price row tagged with this Flyer's Id (Price.FlyerId), not
    /// a separate line-item table. There is no pending/moderation state:
    /// creating a Flyer *is* publishing it. Deliberately NOT IMustHaveTenant/
    /// IMayHaveTenant - part of the same shared public catalog as Store/
    /// Product/Price (see Store.cs's doc comment for why).
    /// </summary>
    [Table("Flyers")]
    public class Flyer : FullAuditedEntity<Guid>
    {
        public Guid StoreId { get; set; }
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        public Guid ImageId { get; set; }
        [ForeignKey("ImageId")]
        public virtual BinaryObject Image { get; set; }

        /// <summary>Always an admin today (see FlyerAppService's doc comment) - kept nullable/named generically in case a self-service path is reintroduced later.</summary>
        public long? UploadedByUserId { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
