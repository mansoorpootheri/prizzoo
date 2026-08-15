using Abp.Application.Services.Dto;
using Abp.Application.Editions;
using EnterpriseBase.MultiTenancy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.Subscriptions.Dto
{
    public class EditionInfoDto : EntityDto<int>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

    // Tenant submits this — creates a pending request
    public class RequestSubscriptionInput
    {
        [Required]
        public int EditionId { get; set; }
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

        [MaxLength(200)]
        public string Notes { get; set; }
    }

    // Host activates a pending request — can override start/end date
    public class ActivateSubscriptionInput
    {
        [Required]
        public long RequestId { get; set; }

        // Host can override the calculated dates
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
    }

    // Host extends an active subscription's end date or trial
    public class ExtendSubscriptionInput
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public DateTime NewEndDateUtc { get; set; }

        public bool IsTrialExtension { get; set; } = false;

        [MaxLength(500)]
        public string Reason { get; set; }
    }

    // Host rejects a pending request
    public class RejectSubscriptionInput
    {
        [Required]
        public long RequestId { get; set; }

        [MaxLength(500)]
        public string Reason { get; set; }
    }

    // Keep for backward compat but now just calls RequestSubscription internally
    public class UpgradeTenantToEditionInput
    {
        [Required]
        public int EditionId { get; set; }
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
        public bool IsInTrialPeriod { get; set; }
    }

    public class SubscriptionRequestDto : EntityDto<long>
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public int EditionId { get; set; }
        public string EditionName { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public SubscriptionRequestStatus Status { get; set; }
        public decimal AmountDue { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public DateTime? ActivatedOnUtc { get; set; }
        public string RejectionReason { get; set; }
        public string Notes { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class AvailablePlanDto : EntityDto<int>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsCurrent { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public decimal? AnnualPrice { get; set; }
        public decimal GstRate { get; set; }
        public bool IsPriceInclusiveOfGst { get; set; }
        public string HsnSacCode { get; set; }
        public int? TrialDayCount { get; set; }
        public bool IsFree { get; set; }
        public decimal? MonthlyPriceExclGst { get; set; }
        public decimal? MonthlyGstAmount { get; set; }
        public decimal? AnnualPriceExclGst { get; set; }
        public decimal? AnnualGstAmount { get; set; }
        public SubscriptionRequestStatus? PendingRequestStatus { get; set; }
    }

    public class AvailablePlansOutput
    {
        public int? CurrentEditionId { get; set; }
        public BillingCycle? CurrentBillingCycle { get; set; }
        public List<AvailablePlanDto> Plans { get; set; }
    }

    public class GetCurrentEditionOutput
    {
        public EditionInfoDto Edition { get; set; }
        public DateTime? SubscriptionEndDateUtc { get; set; }
        public bool IsInTrialPeriod { get; set; }
        public int DaysLeft { get; set; }
        public BillingCycle? BillingCycle { get; set; }
        public decimal? LastAmountPaid { get; set; }
        public DateTime? LastBillingDateUtc { get; set; }
        public SubscriptionRequestDto PendingRequest { get; set; }
    }
}