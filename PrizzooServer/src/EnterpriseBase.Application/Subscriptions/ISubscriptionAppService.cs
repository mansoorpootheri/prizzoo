using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseBase.Application.Subscriptions.Dto;

namespace EnterpriseBase.Application.Subscriptions
{
    public interface ISubscriptionAppService : IApplicationService
    {
        // Tenant actions
        Task<SubscriptionRequestDto> RequestSubscription(RequestSubscriptionInput input);
        Task<GetCurrentEditionOutput> GetCurrentEdition();
        Task<AvailablePlansOutput> GetAvailablePlans();

        // Host actions
        Task<SubscriptionRequestDto> ActivateSubscription(ActivateSubscriptionInput input);
        Task<SubscriptionRequestDto> RejectSubscription(RejectSubscriptionInput input);
        Task ExtendSubscription(ExtendSubscriptionInput input);
        Task<List<SubscriptionRequestDto>> GetPendingRequests();
        Task<PagedResultDto<SubscriptionRequestDto>> GetAllRequests(PagedResultRequestDto input);
    }
}
