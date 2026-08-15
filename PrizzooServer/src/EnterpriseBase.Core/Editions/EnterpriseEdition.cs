using Abp.Application.Editions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Editions
{
    public class EnterpriseEdition : Edition
    {
        // Pricing (INR)
        public decimal? MonthlyPrice { get; set; }
        public decimal? AnnualPrice { get; set; }

        // Indian GST
        [Column(TypeName = "decimal(5,2)")]
        public decimal GstRate { get; set; } = 18m; // 18% GST on SaaS

        public bool IsPriceInclusiveOfGst { get; set; } = false;

        [MaxLength(20)]
        public string HsnSacCode { get; set; } = "998314"; // SAC code for SaaS/software services

        // Trial
        public int? TrialDayCount { get; set; }

        // After expiry, downgrade to this edition (null = no downgrade)
        public int? ExpiringEditionId { get; set; }

        public bool IsFree => !MonthlyPrice.HasValue || MonthlyPrice == 0;

        // Computed helpers
        public decimal? MonthlyPriceExclGst => IsPriceInclusiveOfGst && MonthlyPrice.HasValue
            ? MonthlyPrice.Value / (1 + GstRate / 100)
            : MonthlyPrice;

        public decimal? MonthlyGstAmount => MonthlyPriceExclGst.HasValue
            ? MonthlyPriceExclGst.Value * GstRate / 100
            : null;

        public decimal? AnnualPriceExclGst => IsPriceInclusiveOfGst && AnnualPrice.HasValue
            ? AnnualPrice.Value / (1 + GstRate / 100)
            : AnnualPrice;

        public decimal? AnnualGstAmount => AnnualPriceExclGst.HasValue
            ? AnnualPriceExclGst.Value * GstRate / 100
            : null;
    }
}
