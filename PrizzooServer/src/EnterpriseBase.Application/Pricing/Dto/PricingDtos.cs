using Abp.Application.Services.Dto;
using EnterpriseBase.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.Pricing.Dto
{
    /// <summary>
    /// One row in the public price-comparison result list - a single store's
    /// current approved price for the searched product, ranked by the caller.
    /// </summary>
    public class StorePriceResultDto
    {
        public Guid PriceId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public Guid? ProductImageId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; }
        public Guid? StoreImageId { get; set; }
        public string StoreAddress { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double DistanceKm { get; set; }
        public decimal Amount { get; set; }

        /// <summary>MRP/original price, if the submitter provided one. Only meaningful when greater than Amount.</summary>
        public decimal? OriginalAmount { get; set; }

        /// <summary>Derived from Amount vs OriginalAmount, rounded to whole percent. Null when there's no original price to compare against.</summary>
        public int? DiscountPercent { get; set; }

        public string Currency { get; set; }
        public DateTime ObservedAt { get; set; }
        public bool IsStale { get; set; } // ObservedAt older than the freshness threshold

        /// <summary>Average of all shopper star ratings (1-5) for this product. Null when RatingCount is 0.</summary>
        public double? AverageRating { get; set; }
        public int RatingCount { get; set; }
    }

    /// <summary>
    /// Public, anonymous search input - no auth required. See
    /// PriceCompareAppService for why this stays a separate, unauthenticated
    /// service from the shopper/retailer/admin-scoped ones.
    /// </summary>
    public class ComparePricesInputDto
    {
        /// <summary>
        /// Free-text match against the product name. Optional when
        /// CategoryName is set instead - the home screen's category chips
        /// browse a whole category rather than searching by name, and a
        /// category name (e.g. "Electronics & Mobiles") is never itself a
        /// substring of any product name, so it must never be sent here.
        /// At least one of ProductKeyword/CategoryName should be set, or
        /// every approved nearby price is returned.
        /// </summary>
        public string ProductKeyword { get; set; }

        /// <summary>Exact (case-insensitive) catalog category name to browse - see ProductKeyword.</summary>
        public string CategoryName { get; set; }

        /// <summary>Browse everything one specific store sells - the home screen's retailer strip. See ProductKeyword.</summary>
        public Guid? StoreId { get; set; }

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        /// <summary>
        /// Search radius in kilometres. Defaults to city-wide rather than
        /// city-block: Kozhikode's own outskirts (e.g. Feroke) are ~11km
        /// from the city center, so a tighter default would silently hide
        /// real, approved, nearby stores from most searches.
        /// </summary>
        public double RadiusKm { get; set; } = 15;

        public int MaxResults { get; set; } = 20;
    }

    /// <summary>
    /// Home screen input - same shape as ComparePricesInputDto minus the
    /// keyword, since this browses everything nearby rather than searching.
    /// </summary>
    public class HomeFeedInputDto
    {
        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        public double RadiusKm { get; set; } = 15;
    }

    /// <summary>One horizontal row on the home screen: "Top Picks for You" or a catalog category name.</summary>
    public class HomeFeedSectionDto
    {
        public string Title { get; set; }
        public List<StorePriceResultDto> Items { get; set; }
    }

    /// <summary>Shopper-submitted crowdsourced price, pending moderation.</summary>
    public class SubmitPriceDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid StoreId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>Optional MRP/original price, for computing a discount badge. Must exceed Amount to have any effect.</summary>
        [Range(0.01, double.MaxValue)]
        public decimal? OriginalAmount { get; set; }

        public Guid? ProofImageId { get; set; }
    }

    public class ModeratePriceDto : EntityDto<Guid>
    {
        [Required]
        public PriceStatus Status { get; set; }

        public string ModerationNote { get; set; }
    }

    public class PendingPriceDto : EntityDto<Guid>
    {
        public string ProductName { get; set; }
        public string StoreName { get; set; }
        public decimal Amount { get; set; }
        public Guid? ProofImageId { get; set; }
        public DateTime ObservedAt { get; set; }
    }
}
