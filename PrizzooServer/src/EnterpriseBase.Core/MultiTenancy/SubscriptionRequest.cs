using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.MultiTenancy
{
    public enum SubscriptionRequestStatus
    {
        Pending = 1,
        Activated = 2,
        Rejected = 3
    }

    public class SubscriptionRequest : FullAuditedEntity<long>
    {
        public int TenantId { get; set; }
        public int EditionId { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public SubscriptionRequestStatus Status { get; set; } = SubscriptionRequestStatus.Pending;

        // Amount to be paid (calculated at request time)
        public decimal AmountDue { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Subscription period — set by host on activation
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }

        // Set by host on activation
        public DateTime? ActivatedOnUtc { get; set; }

        [MaxLength(500)]
        public string RejectionReason { get; set; }

        [MaxLength(200)]
        public string Notes { get; set; }
    }
}
