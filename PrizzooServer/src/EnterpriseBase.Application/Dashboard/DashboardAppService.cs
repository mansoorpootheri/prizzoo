using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EnterpriseBase.Application.Dashboard.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.MasterData;
using EnterpriseBase.MultiTenancy;
using EnterpriseBase.Pricing;
using EnterpriseBase.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Dashboard
{
    [AbpAuthorize]
    public class DashboardAppService : ApplicationService, IDashboardAppService
    {
        private readonly IRepository<Store, Guid> _storeRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Price, Guid> _priceRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<User, long> _userRepository;

        public DashboardAppService(
            IRepository<Store, Guid> storeRepository,
            IRepository<Product, Guid> productRepository,
            IRepository<Price, Guid> priceRepository,
            IRepository<Tenant, int> tenantRepository,
            IRepository<User, long> userRepository)
        {
            _storeRepository = storeRepository;
            _productRepository = productRepository;
            _priceRepository = priceRepository;
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
        }

        // Internal ops dashboard - catalog and moderation health, not billing.
        // Replaces the old invoice/revenue TenantDashboardDto (see git history /
        // DOCS/PRIZZOO_MIGRATION_NOTES.md for why).
        [AbpAuthorize(PermissionNames.Pages_Tenant_Dashboard)]
        public async Task<OpsDashboardDto> GetOpsDashboardAsync()
        {
            var totalStores = await _storeRepository.CountAsync(x => x.IsActive);
            var verifiedStores = await _storeRepository.CountAsync(x => x.IsActive && x.IsVerified);
            var totalProducts = await _productRepository.CountAsync(x => x.IsActive);
            var pendingModerations = await _priceRepository.CountAsync(x => x.Status == PriceStatus.Pending);

            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var approvedLast7Days = await _priceRepository.CountAsync(
                x => x.Status == PriceStatus.Approved && x.ObservedAt >= sevenDaysAgo);

            var recentSubmissions = await _priceRepository.GetAll()
                .Include(x => x.Product)
                .Include(x => x.Store)
                .Where(x => x.Status == PriceStatus.Pending)
                .OrderByDescending(x => x.ObservedAt)
                .Take(10)
                .Select(x => new RecentPriceSubmissionDto
                {
                    Id = x.Id,
                    ProductName = x.Product.Name,
                    StoreName = x.Store.Name,
                    Amount = x.Amount,
                    Status = (int)x.Status,
                    ObservedAt = x.ObservedAt,
                })
                .ToListAsync();

            return new OpsDashboardDto
            {
                TotalStores = totalStores,
                VerifiedStores = verifiedStores,
                TotalProducts = totalProducts,
                PendingPriceModerations = pendingModerations,
                ApprovedPricesLast7Days = approvedLast7Days,
                RecentSubmissions = recentSubmissions,
            };
        }

        [AbpAuthorize(PermissionNames.Pages_Tenants)]
        public async Task<HostDashboardDto> GetHostDashboardAsync()
        {
            var totalTenants = await _tenantRepository.CountAsync();
            var activeTenants = await _tenantRepository.CountAsync(x => x.IsActive);
            var totalUsers = await _userRepository.CountAsync();

            var recentTenants = await _tenantRepository.GetAll()
                .OrderByDescending(x => x.CreationTime)
                .Take(5)
                .Select(x => new RecentTenantDto
                {
                    Id = x.Id,
                    TenancyName = x.TenancyName,
                    Name = x.Name,
                    IsActive = x.IsActive,
                })
                .ToListAsync();

            return new HostDashboardDto
            {
                TotalTenants = totalTenants,
                ActiveTenants = activeTenants,
                TotalUsers = totalUsers,
                RecentTenants = recentTenants,
            };
        }
    }
}
