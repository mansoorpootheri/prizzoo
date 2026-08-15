using Abp.Application.Services;
using EnterpriseBase.Configuration.Host.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Configuration.Host
{
    public interface IHostSettingsAppService : IApplicationService
    {
        Task<HostSettingsEditDto> GetAllSettings();

        Task UpdateAllSettings(HostSettingsEditDto input);

        Task SendTestEmail(SendTestEmailInput input);

        Task<List<DatabaseInfoDto>> GetDatabasesList();

        Task<DatabaseBackupResultDto> BackupDatabase(BackupDatabaseInput input);

        Task<bool> GetBackupStatus(string fileToken);
    }
}
