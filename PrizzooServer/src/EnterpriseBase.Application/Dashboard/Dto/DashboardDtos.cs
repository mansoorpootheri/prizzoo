using System;
using System.Collections.Generic;

namespace EnterpriseBase.Application.Dashboard.Dto
{
    // Replaces the old invoice/revenue TenantDashboardDto - Prizzoo's internal
    // ops dashboard tracks catalog and moderation health instead of billing.
    public class OpsDashboardDto
    {
        public int TotalStores { get; set; }
        public int VerifiedStores { get; set; }
        public int TotalProducts { get; set; }
        public int PendingPriceModerations { get; set; }
        public int ApprovedPricesLast7Days { get; set; }
        public List<RecentPriceSubmissionDto> RecentSubmissions { get; set; } = new();
    }

    public class RecentPriceSubmissionDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string StoreName { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public DateTime ObservedAt { get; set; }
    }

    public class HostDashboardDto
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public int TotalUsers { get; set; }
        public List<RecentTenantDto> RecentTenants { get; set; } = new();
    }

    public class RecentTenantDto
    {
        public int Id { get; set; }
        public string TenancyName { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
