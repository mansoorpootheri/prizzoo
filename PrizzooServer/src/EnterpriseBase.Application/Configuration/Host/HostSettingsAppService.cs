using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Net.Mail;
using Abp.Runtime.Security;
using Abp.Timing;
using Abp.UI;
using EnterpriseBase.Authorization;
using EnterpriseBase.Configuration.Dto;
using EnterpriseBase.Configuration.Host.Dto;
using EnterpriseBase.Editions;
using EnterpriseBase.MultiTenancy;
using EnterpriseBase.Timing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Configuration.Host
{
    [AbpAuthorize(PermissionNames.Pages_Administration_Host_Settings)]
    public class HostSettingsAppService : SettingsAppServiceBase, IHostSettingsAppService
    {
        private readonly ITimeZoneService _timeZoneService;
        private readonly IAppConfigurationAccessor _configurationAccessor;
        private readonly IRepository<Tenant, int> _tenantRepository;

        public HostSettingsAppService(
            IEmailSender emailSender,
            IAppConfigurationAccessor configurationAccessor,
            ITimeZoneService timeZoneService,
            IRepository<Tenant, int> tenantRepository) : base(emailSender, configurationAccessor)
        {
            _timeZoneService = timeZoneService;
            _configurationAccessor = configurationAccessor;
            _tenantRepository = tenantRepository;
        }
        public async Task<HostSettingsEditDto> GetAllSettings()
        {
            return new HostSettingsEditDto
            {
                General= await GetGeneralSettingsAsync(),
                TenantManagement = await GetTenantManagementSettingsAsync(),
                Ui = await GetUiSettingsAsync()
            };
        }

        public async Task UpdateAllSettings(HostSettingsEditDto input)
        {
            await UpdateGeneralSettingsAsync(input.General);
            await UpdateTenantManagementAsync(input.TenantManagement);
            await UpdateUiSettingsAsync(input.Ui);
        }


        private async Task<GeneralSettingsEditDto> GetGeneralSettingsAsync()
        {
            var timezone = await SettingManager.GetSettingValueForApplicationAsync(TimingSettingNames.TimeZone);
            var settings = new GeneralSettingsEditDto
            {
                Timezone = timezone,
                TimezoneForComparison = timezone
            };

            var defaultTimeZoneId =
                await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Application, AbpSession.TenantId);
            if (settings.Timezone == defaultTimeZoneId)
            {
                settings.Timezone = string.Empty;
            }

            return settings;
        }

        private async Task<UiSettingsEditDto> GetUiSettingsAsync()
        {
            return new UiSettingsEditDto
            {
                SearchActive = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UiManagement.SearchActive)
            };
        }

        private async Task<TenantManagementSettingsEditDto> GetTenantManagementSettingsAsync()
        {
            var settings = new TenantManagementSettingsEditDto
            {
                AllowSelfRegistration =
                    await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement.AllowSelfRegistration),
                //IsNewRegisteredTenantActiveByDefault =
                //    await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement
                //        .IsNewRegisteredTenantActiveByDefault),

                //CaptchaSettings = new CaptchaSettingsEditDto()
                //{
                //    UseCaptchaOnRegistration =
                //    await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement
                //        .UseCaptchaOnRegistration),
                //    UseCaptchaOnEmailActivation =
                //    await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement
                //        .UseCaptchaOnEmailActivation),
                //    UseCaptchaOnResetPassword =
                //    await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement
                //        .UseCaptchaOnResetPassword),
                //},

                //IsRestrictedEmailDomainEnabled =
                //    await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement.IsRestrictedEmailDomainEnabled),
            };

            return settings;
        }
        private async Task UpdateGeneralSettingsAsync(GeneralSettingsEditDto settings)
        {
            if (Clock.SupportsMultipleTimezone)
            {
                if (settings.Timezone.IsNullOrEmpty())
                {
                    var defaultValue =
                       await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Application, AbpSession.TenantId);
                    await SettingManager.ChangeSettingForApplicationAsync(TimingSettingNames.TimeZone, defaultValue);
                }
                else
                {
                    _timeZoneService.ValidateTimezone(settings.Timezone);
                    await SettingManager.ChangeSettingForApplicationAsync(TimingSettingNames.TimeZone,
                        settings.Timezone);
                }
            }
        }

        private async Task UpdateUiSettingsAsync(UiSettingsEditDto settings)
        {
            await SettingManager.ChangeSettingForApplicationAsync(
                AppSettings.UiManagement.SearchActive,
                settings.SearchActive.ToString().ToLowerInvariant()
            );
        }

        private async Task UpdateTenantManagementAsync(TenantManagementSettingsEditDto settings)
        {
            await SettingManager.ChangeSettingForApplicationAsync(
                AppSettings.TenantManagement.AllowSelfRegistration,
                settings.AllowSelfRegistration.ToString().ToLowerInvariant()
            );
        }

        #region Database Backup

        public async Task<List<DatabaseInfoDto>> GetDatabasesList()
        {
            var result = new List<DatabaseInfoDto>();

            // Host DB
            var hostConnStr = _configurationAccessor.Configuration["ConnectionStrings:Default"];
            var hostDbName = ExtractDatabaseName(hostConnStr);
            result.Add(new DatabaseInfoDto { DatabaseName = hostDbName, TenantName = null });

            // Tenant DBs (only those with their own connection string)
            var tenants = await _tenantRepository.GetAll()
                .Where(t => t.ConnectionString != null && t.ConnectionString != "")
                .ToListAsync();

            foreach (var tenant in tenants)
            {
                var decrypted = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                var dbName = ExtractDatabaseName(decrypted);
                if (!result.Any(r => r.DatabaseName == dbName))
                {
                    result.Add(new DatabaseInfoDto { DatabaseName = dbName, TenantName = tenant.Name });
                }
            }

            return result;
        }

        public async Task<DatabaseBackupResultDto> BackupDatabase(BackupDatabaseInput input)
        {
            if (string.IsNullOrWhiteSpace(input?.DatabaseName))
                throw new UserFriendlyException("Database name is required.");

            // Verify the requested DB is in the allowed list
            var allowedDbs = await GetDatabasesList();
            if (!allowedDbs.Any(d => d.DatabaseName == input.DatabaseName))
                throw new UserFriendlyException("Database not found or not accessible.");

            // Get connection string for the requested DB
            var connStr = GetConnectionStringForDatabase(input.DatabaseName, allowedDbs);
            var (host, port, user, password) = ParseConnectionString(connStr);

            var fileToken = Guid.NewGuid().ToString("N");
            var fileName = $"{input.DatabaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.backup";
            var tempPath = Path.Combine(Path.GetTempPath(), fileToken + ".backup");

            Logger.Info($"[BackupDatabase] Starting backup for database '{input.DatabaseName}'");

            // Run pg_dump in background
            _ = Task.Run(() => RunPgDump(host, port, user, password, input.DatabaseName, tempPath));

            return new DatabaseBackupResultDto
            {
                FileToken = fileToken,
                FileName = fileName
            };
        }

        public Task<bool> GetBackupStatus(string fileToken)
        {
            if (string.IsNullOrWhiteSpace(fileToken))
                throw new UserFriendlyException("File token is required.");

            // Validate token format (should be 32 hex chars)
            if (fileToken.Length != 32 || !fileToken.All(c => char.IsLetterOrDigit(c)))
                throw new UserFriendlyException("Invalid file token.");

            var tempPath = Path.Combine(Path.GetTempPath(), fileToken + ".backup");
            var markerPath = tempPath + ".done";

            if (File.Exists(markerPath))
            {
                return Task.FromResult(true);
            }

            // Check if failed
            var errorPath = tempPath + ".error";
            if (File.Exists(errorPath))
            {
                var error = File.ReadAllText(errorPath);
                File.Delete(errorPath);
                throw new UserFriendlyException($"Backup failed: {error}");
            }

            return Task.FromResult(false);
        }

        private static readonly string PgDumpPath = FindPgDump();

        private static string FindPgDump()
        {
            // Azure App Service
            var home = Environment.ExpandEnvironmentVariables("%HOME%");
            var appServicePgDump = Path.Combine(home, "pg_dumps", "pg_dump.exe");
            if (File.Exists(appServicePgDump))
            {
                return appServicePgDump;
            }

            // Local PostgreSQL installation
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var pgDir = Path.Combine(programFiles, "PostgreSQL");
            if (Directory.Exists(pgDir))
            {
                foreach (var version in Directory.GetDirectories(pgDir).OrderByDescending(x => x))
                {
                    var candidate = Path.Combine(version, "bin", "pg_dump.exe");
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }

            // PATH
            return "pg_dump";
        }

        private void RunPgDump(string host, string port, string user, string password, string dbName, string outputPath)
        {
            try
            {
                Logger.Info($"Using pg_dump: {PgDumpPath}");
                Logger.Info($"Starting pg_dump for database '{dbName}' to '{outputPath}'");

                var startInfo = new ProcessStartInfo
                {
                    FileName = PgDumpPath,
                    Arguments = $"-h {host} -p {port} -U {user} -Fc \"{dbName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // If pg_dump is an absolute path (Azure/App Service or local install),
                // use its folder as the working directory so DLLs can be found.
                if (Path.IsPathRooted(PgDumpPath))
                {
                    startInfo.WorkingDirectory = Path.GetDirectoryName(PgDumpPath);
                }

                startInfo.EnvironmentVariables["PGPASSWORD"] = password;

                using (var process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();

                    var errorTask = process.StandardError.ReadToEndAsync();

                    using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        process.StandardOutput.BaseStream.CopyTo(fileStream);
                    }

                    var exited = process.WaitForExit(600000);
                    var error = errorTask.Result;

                    if (!exited)
                    {
                        process.Kill();
                        File.WriteAllText(outputPath + ".error", "Backup timed out after 10 minutes.");
                        return;
                    }

                    if (process.ExitCode == 0)
                    {
                        File.WriteAllText(outputPath + ".done", "ok");
                    }
                    else
                    {
                        File.WriteAllText(outputPath + ".error",
                            string.IsNullOrWhiteSpace(error)
                                ? $"pg_dump exited with code {process.ExitCode}."
                                : error);
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                var msg = $"Unable to start pg_dump. Resolved path: '{PgDumpPath}'. {ex.Message}";
                Logger.Error(msg, ex);
                File.WriteAllText(outputPath + ".error", msg);
            }
            catch (Exception ex)
            {
                Logger.Error($"pg_dump unexpected error for database '{dbName}'", ex);
                File.WriteAllText(outputPath + ".error", ex.ToString());
            }
        }

        private string GetConnectionStringForDatabase(string databaseName, List<DatabaseInfoDto> allowedDbs)
        {
            var hostConnStr = _configurationAccessor.Configuration["ConnectionStrings:Default"];
            if (ExtractDatabaseName(hostConnStr) == databaseName)
                return hostConnStr;

            var tenants = _tenantRepository.GetAll()
                .Where(t => t.ConnectionString != null && t.ConnectionString != "")
                .ToList();

            foreach (var tenant in tenants)
            {
                var decrypted = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                if (ExtractDatabaseName(decrypted) == databaseName)
                    return decrypted;
            }

            throw new UserFriendlyException("Connection string not found for the specified database.");
        }

        private static string ExtractDatabaseName(string connectionString)
        {
            // Parse "Database=xyz" from connection string
            var parts = connectionString.Split(';')
                .Select(p => p.Trim())
                .Where(p => p.StartsWith("Database", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            return parts?.Split('=')[1]?.Trim() ?? "";
        }

        private static (string host, string port, string user, string password) ParseConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p) && p.Contains('='))
                .ToList();

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in parts)
            {
                var idx = part.IndexOf('=');
                if (idx > 0)
                {
                    var key = part.Substring(0, idx).Trim();
                    var value = part.Substring(idx + 1).Trim();
                    dict[key] = value;
                }
            }

            dict.TryGetValue("Host", out var host);
            if (host == null) dict.TryGetValue("Server", out host);

            dict.TryGetValue("Port", out var port);

            dict.TryGetValue("User ID", out var user);
            if (user == null) dict.TryGetValue("Username", out user);
            if (user == null) dict.TryGetValue("User", out user);

            dict.TryGetValue("Password", out var password);

            return (host ?? "localhost", port ?? "5432", user ?? "postgres", password ?? "");
        }

        #endregion
    }
}
