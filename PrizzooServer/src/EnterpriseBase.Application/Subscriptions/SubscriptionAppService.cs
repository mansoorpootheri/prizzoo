using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Application.Editions;
using Abp.UI;
using EnterpriseBase.Authorization;
using EnterpriseBase.Editions;
using EnterpriseBase.MultiTenancy;
using EnterpriseBase.Application.Subscriptions.Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Abp.Linq.Extensions;

namespace EnterpriseBase.Application.Subscriptions
{
    [AbpAuthorize]
    public class SubscriptionAppService : ApplicationService, ISubscriptionAppService
    {
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<Edition, int> _editionRepository;
        private readonly IRepository<SubscriptionRequest, long> _requestRepository;
        private readonly TenantManager _tenantManager;

        public SubscriptionAppService(
            IRepository<Tenant, int> tenantRepository,
            IRepository<Edition, int> editionRepository,
            IRepository<SubscriptionRequest, long> requestRepository,
            TenantManager tenantManager)
        {
            _tenantRepository = tenantRepository;
            _editionRepository = editionRepository;
            _requestRepository = requestRepository;
            _tenantManager = tenantManager;
        }

        /// <summary>
        /// Tenant requests a plan change — creates a Pending request for host to review
        /// </summary>
        public async Task<SubscriptionRequestDto> RequestSubscription(RequestSubscriptionInput input)
        {
            var tenant = await GetCurrentTenantAsync()
                ?? throw new UserFriendlyException("This operation is only available for tenants.");

            var edition = await _editionRepository.GetAll()
                .OfType<EnterpriseEdition>()
                .FirstOrDefaultAsync(e => e.Id == input.EditionId)
                ?? throw new UserFriendlyException("Edition not found.");

            // Cancel any existing pending request for this tenant
            var existingPending = await _requestRepository.GetAll()
                .Where(r => r.TenantId == tenant.Id && r.Status == SubscriptionRequestStatus.Pending)
                .ToListAsync();
            foreach (var r in existingPending)
            {
                r.Status = SubscriptionRequestStatus.Rejected;
                r.RejectionReason = "Superseded by new request";
                await _requestRepository.UpdateAsync(r);
            }

            // Calculate amounts
            decimal baseAmount, gstAmount, totalAmount;
            if (edition.IsFree)
            {
                baseAmount = gstAmount = totalAmount = 0;
            }
            else if (input.BillingCycle == BillingCycle.Yearly)
            {
                baseAmount = edition.AnnualPriceExclGst ?? 0;
                gstAmount = edition.AnnualGstAmount ?? 0;
                totalAmount = edition.IsPriceInclusiveOfGst
                    ? edition.AnnualPrice ?? 0
                    : baseAmount + gstAmount;
            }
            else
            {
                baseAmount = edition.MonthlyPriceExclGst ?? 0;
                gstAmount = edition.MonthlyGstAmount ?? 0;
                totalAmount = edition.IsPriceInclusiveOfGst
                    ? edition.MonthlyPrice ?? 0
                    : baseAmount + gstAmount;
            }

            var request = new SubscriptionRequest
            {
                TenantId = tenant.Id,
                EditionId = input.EditionId,
                BillingCycle = edition.IsFree ? BillingCycle.Monthly : input.BillingCycle,
                Status = edition.IsFree ? SubscriptionRequestStatus.Activated : SubscriptionRequestStatus.Pending,
                AmountDue = baseAmount,
                GstAmount = gstAmount,
                TotalAmount = totalAmount,
                Notes = input.Notes
            };

            await _requestRepository.InsertAsync(request);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Free plan — activate immediately, no host approval needed
            if (edition.IsFree)
            {
                var now = DateTime.UtcNow;
                request.StartDateUtc = now;
                request.EndDateUtc = null;
                await ActivateTenant(tenant, edition, request, now, null);
            }

            return await MapToDto(request);
        }

        /// <summary>
        /// Host activates a pending subscription request
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_Tenants)]
        public async Task<SubscriptionRequestDto> ActivateSubscription(ActivateSubscriptionInput input)
        {
            var request = await _requestRepository.GetAsync(input.RequestId);

            if (request.Status != SubscriptionRequestStatus.Pending)
                throw new UserFriendlyException("Only pending requests can be activated.");

            var tenant = await _tenantRepository.GetAsync(request.TenantId);
            var edition = await _editionRepository.GetAll()
                .OfType<EnterpriseEdition>()
                .FirstOrDefaultAsync(e => e.Id == request.EditionId)
                ?? throw new UserFriendlyException("Edition not found.");

            var now = DateTime.UtcNow;
            var startDate = input.StartDateUtc ?? now;
            DateTime? endDate = input.EndDateUtc;

            // If host didn't override end date, calculate from billing cycle
            if (endDate == null && !edition.IsFree)
            {
                endDate = request.BillingCycle == BillingCycle.Yearly
                    ? startDate.AddYears(1)
                    : startDate.AddMonths(1);
            }

            request.StartDateUtc = startDate;
            request.EndDateUtc = endDate;

            await ActivateTenant(tenant, edition, request, startDate, endDate);

            return await MapToDto(request);
        }

        /// <summary>
        /// Host rejects a pending subscription request
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_Tenants)]
        public async Task<SubscriptionRequestDto> RejectSubscription(RejectSubscriptionInput input)
        {
            var request = await _requestRepository.GetAsync(input.RequestId);

            if (request.Status != SubscriptionRequestStatus.Pending)
                throw new UserFriendlyException("Only pending requests can be rejected.");

            request.Status = SubscriptionRequestStatus.Rejected;
            request.RejectionReason = input.Reason;
            await _requestRepository.UpdateAsync(request);

            return await MapToDto(request);
        }

        /// <summary>
        /// Host gets all pending subscription requests
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_Tenants)]
        public async Task<List<SubscriptionRequestDto>> GetPendingRequests()
        {
            var requests = await _requestRepository.GetAll()
                .Where(r => r.Status == SubscriptionRequestStatus.Pending)
                .OrderByDescending(r => r.CreationTime)
                .ToListAsync();

            var result = new List<SubscriptionRequestDto>();
            foreach (var r in requests)
                result.Add(await MapToDto(r));
            return result;
        }

        /// <summary>
        /// Host gets all subscription requests (all statuses)
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_Tenants)]
        public async Task<PagedResultDto<SubscriptionRequestDto>> GetAllRequests(PagedResultRequestDto input)
        {
            var query = _requestRepository.GetAll().OrderByDescending(r => r.CreationTime);
            var total = await query.CountAsync();
            var requests = await query.PageBy(input).ToListAsync();

            var result = new List<SubscriptionRequestDto>();
            foreach (var r in requests)
                result.Add(await MapToDto(r));

            return new PagedResultDto<SubscriptionRequestDto>(total, result);
        }

        /// <summary>
        /// Get current tenant's edition + pending request info
        /// </summary>
        public async Task<GetCurrentEditionOutput> GetCurrentEdition()
        {
            var tenant = await GetCurrentTenantAsync()
                ?? throw new UserFriendlyException("Tenant not found.");

            Edition edition = null;
            if (tenant.EditionId.HasValue)
                edition = await _editionRepository.GetAsync(tenant.EditionId.Value);

            // Check if there's a pending request
            var pendingRequest = await _requestRepository.GetAll()
                .Where(r => r.TenantId == tenant.Id && r.Status == SubscriptionRequestStatus.Pending)
                .OrderByDescending(r => r.CreationTime)
                .FirstOrDefaultAsync();

            return new GetCurrentEditionOutput
            {
                Edition = edition != null ? ObjectMapper.Map<EditionInfoDto>(edition) : null,
                SubscriptionEndDateUtc = tenant.SubscriptionEndDateUtc,
                IsInTrialPeriod = tenant.IsInTrialPeriod,
                DaysLeft = tenant.CalculateRemainingHoursCount() / 24,
                BillingCycle = tenant.BillingCycle,
                LastAmountPaid = tenant.LastAmountPaid,
                LastBillingDateUtc = tenant.LastBillingDateUtc,
                PendingRequest = pendingRequest != null ? await MapToDto(pendingRequest) : null
            };
        }

        /// <summary>
        /// Get available plans — also shows if tenant has a pending request on a plan
        /// </summary>
        public async Task<AvailablePlansOutput> GetAvailablePlans()
        {
            var tenant = await GetCurrentTenantAsync()
                ?? throw new UserFriendlyException("This operation is only available for tenants.");

            var editions = await _editionRepository.GetAll()
                .OfType<EnterpriseEdition>()
                .OrderBy(e => e.DisplayName)
                .ToListAsync();

            // Get latest request per edition for this tenant
            var allRequests = await _requestRepository.GetAll()
                .Where(r => r.TenantId == tenant.Id)
                .OrderByDescending(r => r.CreationTime)
                .ToListAsync();
            var latestRequests = allRequests
                .GroupBy(r => r.EditionId)
                .Select(g => g.First())
                .ToList();

            return new AvailablePlansOutput
            {
                CurrentEditionId = tenant.EditionId,
                CurrentBillingCycle = tenant.BillingCycle,
                Plans = editions.Select(e =>
                {
                    var latestReq = latestRequests.FirstOrDefault(r => r.EditionId == e.Id);
                    return new AvailablePlanDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        DisplayName = e.DisplayName,
                        IsCurrent = e.Id == tenant.EditionId,
                        MonthlyPrice = e.MonthlyPrice,
                        AnnualPrice = e.AnnualPrice,
                        GstRate = e.GstRate,
                        IsPriceInclusiveOfGst = e.IsPriceInclusiveOfGst,
                        HsnSacCode = e.HsnSacCode,
                        TrialDayCount = e.TrialDayCount,
                        IsFree = e.IsFree,
                        MonthlyPriceExclGst = e.MonthlyPriceExclGst,
                        MonthlyGstAmount = e.MonthlyGstAmount,
                        AnnualPriceExclGst = e.AnnualPriceExclGst,
                        AnnualGstAmount = e.AnnualGstAmount,
                        PendingRequestStatus = latestReq?.Status == SubscriptionRequestStatus.Pending
                            ? SubscriptionRequestStatus.Pending
                            : null
                    };
                }).ToList()
            };
        }

        /// <summary>
        /// Host extends an active subscription end date or trial period
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_Tenants)]
        public async Task ExtendSubscription(ExtendSubscriptionInput input)
        {
            var tenant = await _tenantRepository.GetAsync(input.TenantId)
                ?? throw new UserFriendlyException("Tenant not found.");

            tenant.SubscriptionEndDateUtc = input.NewEndDateUtc.ToUniversalTime();

            if (input.IsTrialExtension)
                tenant.IsInTrialPeriod = true;

            await _tenantRepository.UpdateAsync(tenant);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        // ── Private helpers ────────────────────────────────────────────────

        private async Task ActivateTenant(Tenant tenant, EnterpriseEdition edition, SubscriptionRequest request, DateTime startDate, DateTime? endDate)
        {
            var now = DateTime.UtcNow;

            tenant.EditionId = edition.Id;
            tenant.BillingCycle = edition.IsFree ? null : request.BillingCycle;
            tenant.IsInTrialPeriod = false;
            tenant.LastAmountPaid = edition.IsFree ? null : request.TotalAmount;
            tenant.LastBillingDateUtc = edition.IsFree ? null : startDate;
            tenant.SubscriptionEndDateUtc = endDate;

            await _tenantRepository.UpdateAsync(tenant);

            request.Status = SubscriptionRequestStatus.Activated;
            request.ActivatedOnUtc = now;
            request.StartDateUtc = startDate;
            request.EndDateUtc = endDate;
            await _requestRepository.UpdateAsync(request);

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task<SubscriptionRequestDto> MapToDto(SubscriptionRequest r)
        {
            var tenant = await _tenantRepository.FirstOrDefaultAsync(r.TenantId);
            var edition = await _editionRepository.FirstOrDefaultAsync(r.EditionId);
            return new SubscriptionRequestDto
            {
                Id = r.Id,
                TenantId = r.TenantId,
                TenantName = tenant?.Name,
                EditionId = r.EditionId,
                EditionName = edition?.DisplayName,
                BillingCycle = r.BillingCycle,
                Status = r.Status,
                AmountDue = r.AmountDue,
                GstAmount = r.GstAmount,
                TotalAmount = r.TotalAmount,
                StartDateUtc = r.StartDateUtc,
                EndDateUtc = r.EndDateUtc,
                ActivatedOnUtc = r.ActivatedOnUtc,
                RejectionReason = r.RejectionReason,
                Notes = r.Notes,
                CreationTime = r.CreationTime
            };
        }

        private async Task<Tenant> GetCurrentTenantAsync()
        {
            if (!AbpSession.TenantId.HasValue) return null;
            return await _tenantRepository.GetAsync(AbpSession.TenantId.Value);
        }
    }
}
