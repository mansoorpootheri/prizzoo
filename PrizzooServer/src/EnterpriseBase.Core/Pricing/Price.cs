using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using EnterpriseBase.MasterData;
using EnterpriseBase.Stores;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Pricing
{
    public enum PriceSource
    {
        RetailerReported = 1,
        Crowdsourced = 2,
        OndcFeed = 3,
        FieldCollected = 4
    }

    public enum PriceStatus
    {
        Pending = 1,
        Approved = 2,
        Flagged = 3,
        Rejected = 4
    }

    /// <summary>
    /// One product's price at one store, at one point in time. This is the
    /// entity the public comparison search actually ranks and returns -
    /// NOT a field on Product, because the same product has many different
    /// prices across many different stores (the entire premise of Prizzoo).
    ///
    /// Only rows with Status == Approved should be considered "current" and
    /// surfaced to shoppers. Keep history (don't overwrite) so price-history
    /// charts (Phase 2) work without a separate audit table.
    /// </summary>
    [Table("Prices")]
    public class Price : FullAuditedEntity<Guid>
    {
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public Guid StoreId { get; set; }
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The MRP/original price before any discount, if the submitter
        /// provided one. Nullable - most submissions won't have a known
        /// "original" price. Only meaningful when greater than Amount; the
        /// discount percentage shown to shoppers is derived from these two
        /// fields, never stored separately.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalAmount { get; set; }

        [Column(TypeName = "char(3)")]
        public string Currency { get; set; } = "INR";

        public PriceSource Source { get; set; }

        public PriceStatus Status { get; set; } = PriceStatus.Pending;

        /// <summary>
        /// Shopper who submitted this price, for crowdsourced entries.
        /// Null for retailer-reported or field-collected entries.
        /// </summary>
        public long? SubmittedByUserId { get; set; }

        /// <summary>
        /// Photo proof for crowdsourced submissions (receipt / shelf tag).
        /// </summary>
        public Guid? ProofImageId { get; set; }

        /// <summary>
        /// When this price was actually observed/reported, which may differ
        /// from CreationTime if backfilled. Drives the "last updated Xh ago"
        /// freshness indicator that is core to user trust in the product.
        /// </summary>
        public DateTime ObservedAt { get; set; } = DateTime.UtcNow;

        public string ModerationNote { get; set; }
    }
}
