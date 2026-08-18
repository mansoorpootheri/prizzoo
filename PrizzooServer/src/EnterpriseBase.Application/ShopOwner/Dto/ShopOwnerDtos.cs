using EnterpriseBase.Pricing;
using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.ShopOwner.Dto
{
    /// <summary>One of the shop owner's own products, with its latest submitted price (if any).</summary>
    public class MyStoreProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string UnitName { get; set; }
        public Guid? ImageId { get; set; }
        public decimal? LatestPriceAmount { get; set; }
        public PriceStatus? LatestPriceStatus { get; set; }
        public DateTime? LatestPriceObservedAt { get; set; }
    }

    /// <summary>
    /// Submits a new price observation for one of the caller's own products.
    /// Deliberately has no StoreId field - the store is always resolved
    /// server-side from Store.OwnerUserId == the caller, so a shop owner can
    /// never submit a price for a store they don't own.
    /// </summary>
    public class SubmitMyPriceDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>Optional MRP/original price, for computing a discount badge. Must exceed Amount to have any effect.</summary>
        [Range(0.01, double.MaxValue)]
        public decimal? OriginalAmount { get; set; }

        public Guid? ProofImageId { get; set; }
    }
}
