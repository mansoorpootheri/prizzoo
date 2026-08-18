using Abp.Dependency;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Notifications
{
    public interface INotifyShopOwnerService
    {
        /// <summary>Called when a shop owner's price submission is rejected during moderation.</summary>
        Task NotifyPriceRejectedAsync(long ownerUserId, string productName, string moderationNote);
    }

    /// <summary>
    /// TODO: wire up the real WhatsApp Business API integration described in
    /// the business plan for retailer notifications (see
    /// DOCS/PRIZZOO_MIGRATION_NOTES.md - retailer price updates/notices are
    /// meant to go over WhatsApp, not a web dashboard). Until that's built,
    /// this just logs, so PriceSubmissionAppService.ModerateAsync has
    /// somewhere real to call and rejections are at least visible in server
    /// logs during development.
    /// </summary>
    public class NoopNotifyShopOwnerService : INotifyShopOwnerService, ITransientDependency
    {
        private readonly ILogger<NoopNotifyShopOwnerService> _logger;

        public NoopNotifyShopOwnerService(ILogger<NoopNotifyShopOwnerService> logger)
        {
            _logger = logger;
        }

        public Task NotifyPriceRejectedAsync(long ownerUserId, string productName, string moderationNote)
        {
            _logger.LogInformation(
                "[DEV NOTIFY - NoopNotifyShopOwnerService, not real WhatsApp] Would notify shop owner {OwnerUserId} that their price for '{ProductName}' was rejected: {ModerationNote}",
                ownerUserId, productName, moderationNote);
            return Task.CompletedTask;
        }
    }
}
