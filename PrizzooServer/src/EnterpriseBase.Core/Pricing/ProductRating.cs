using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using EnterpriseBase.MasterData;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Pricing
{
    /// <summary>
    /// A shopper's 1-5 star rating of a product (not of a specific store's
    /// price observation - the product listing card shows one aggregate
    /// rating regardless of which store the shopper ends up buying from).
    /// One row per (ProductId, ShopperUserId): rating again overwrites the
    /// existing row rather than inserting a new one, unlike Price which is
    /// deliberately append-only - there's no need for a rating history.
    /// </summary>
    [Table("ProductRatings")]
    public class ProductRating : FullAuditedEntity<Guid>
    {
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public long ShopperUserId { get; set; }

        public int Stars { get; set; }
    }
}
