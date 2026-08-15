using Abp.Application.Services;
using EnterpriseBase.Application.Dashboard.Dto;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Dashboard
{
    public interface IDashboardAppService : IApplicationService
    {
        Task<OpsDashboardDto> GetOpsDashboardAsync();
        Task<HostDashboardDto> GetHostDashboardAsync();
    }
}
