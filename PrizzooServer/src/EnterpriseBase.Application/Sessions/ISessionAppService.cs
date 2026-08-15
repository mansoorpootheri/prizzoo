using Abp.Application.Services;
using EnterpriseBase.Sessions.Dto;
using System.Threading.Tasks;

namespace EnterpriseBase.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
