using Abp.Application.Editions;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.Timing;
using EnterpriseBase.Editions;
using EnterpriseBase.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseBase.Web.Host.Filters
{
    public class SubscriptionExpiryMiddleware : ITransientDependency
    {
        private readonly RequestDelegate _next;

        private static readonly string[] ExemptPaths =
        {
            "/api/services/app/Subscription",
            "/api/TokenAuth",
            "/api/services/app/TokenAuth",
            "/api/services/app/Session",
            "/api/services/app/Account",
            "/api/services/app/Menu",
        };

        public SubscriptionExpiryMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IAbpSession abpSession,
            IRepository<Tenant, int> tenantRepository,
            IRepository<Edition, int> editionRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            // Only process API requests
            var path = context.Request.Path.Value ?? "";
            if (!path.StartsWith("/api/services/app/", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Only check authenticated tenant requests
            if (!abpSession.TenantId.HasValue)
            {
                await _next(context);
                return;
            }

            // Skip exempt API paths
            if (ExemptPaths.Any(e => path.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            Tenant tenant;
            using (var uow = unitOfWorkManager.Begin())
            {
                tenant = await tenantRepository.FirstOrDefaultAsync(abpSession.TenantId.Value);
                await uow.CompleteAsync();
            }

            // No tenant or unlimited subscription
            if (tenant == null || tenant.HasUnlimitedTimeSubscription())
            {
                await _next(context);
                return;
            }

            // Not expired
            if (tenant.SubscriptionEndDateUtc.Value >= Clock.Now.ToUniversalTime())
            {
                await _next(context);
                return;
            }

            // Expired — try auto-downgrade
            if (tenant.EditionId.HasValue)
            {
                EnterpriseEdition currentEdition;
                using (var uow = unitOfWorkManager.Begin())
                {
                    currentEdition = await editionRepository.GetAll()
                        .OfType<EnterpriseEdition>()
                        .FirstOrDefaultAsync(e => e.Id == tenant.EditionId.Value);
                    await uow.CompleteAsync();
                }

                if (currentEdition?.ExpiringEditionId.HasValue == true)
                {
                    using (var uow = unitOfWorkManager.Begin())
                    {
                        tenant.EditionId = currentEdition.ExpiringEditionId.Value;
                        tenant.SubscriptionEndDateUtc = null;
                        tenant.BillingCycle = null;
                        tenant.IsInTrialPeriod = false;
                        await tenantRepository.UpdateAsync(tenant);
                        await uow.CompleteAsync();
                    }

                    await _next(context);
                    return;
                }
            }

            // Block — return proper ABP error response
            context.Response.StatusCode = (int)HttpStatusCode.PaymentRequired;
            context.Response.ContentType = "application/json";

            var response = new
            {
                result = (object)null,
                targetUrl = (object)null,
                success = false,
                error = new
                {
                    code = 0,
                    message = "SubscriptionExpired",
                    details = "Your subscription has expired. Please renew your plan to continue.",
                    validationErrors = (object)null
                },
                unAuthorizedRequest = false,
                __abp = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
