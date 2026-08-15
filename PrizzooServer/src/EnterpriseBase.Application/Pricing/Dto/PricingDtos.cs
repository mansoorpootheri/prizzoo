using Abp.Application.Services.Dto;
using EnterpriseBase.Pricing;
using System;
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
        public Guid StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double DistanceKm { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime ObservedAt { get; set; }
        public bool IsStale { get; set; } // ObservedAt older than the freshness threshold
    }

    /// <summary>
    /// Public, anonymous search input - no auth required. See
    /// PriceCompareAppService for why this stays a separate, unauthenticated
    /// service from the shopper/retailer/admin-scoped ones.
    /// </summary>
    public class ComparePricesInputDto
    {
        [Required]
        public string ProductKeyword { get; set; }

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        /// <summary>Search radius in kilometres. Defaults to a sensible city-block-ish range.</summary>
        public double RadiusKm { get; set; } = 5;

        public int MaxResults { get; set; } = 20;
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
