using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Extensions;
using Abp.Json;
using Abp.Net.Mail;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using Abp.Zero.Configuration;
using EnterpriseBase.Authorization;
using EnterpriseBase.Configuration.Dto;
using EnterpriseBase.Configuration.Host.Dto;
using EnterpriseBase.Configuration.Tenants.Dto;
using EnterpriseBase.Security;
using EnterpriseBase.Storage;
using EnterpriseBase.Timing;

namespace EnterpriseBase.Configuration.Tenants
{
    [AbpAuthorize(PermissionNames.Pages_Administration_Tenant_Settings)]
    public class TenantSettingsAppService : SettingsAppServiceBase, ITenantSettingsAppService
    {
        public IExternalLoginOptionsCacheManager ExternalLoginOptionsCacheManager { get; set; }

        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IBinaryObjectManager _binaryObjectManager;

        public TenantSettingsAppService(
            IMultiTenancyConfig multiTenancyConfig,
            ITimeZoneService timeZoneService,
            IEmailSender emailSender,
            IBinaryObjectManager binaryObjectManager,
            IAppConfigurationAccessor configurationAccessor
        ) : base(emailSender, configurationAccessor)
        {
            ExternalLoginOptionsCacheManager = NullExternalLoginOptionsCacheManager.Instance;
            _multiTenancyConfig = multiTenancyConfig;
            _timeZoneService = timeZoneService;
            _binaryObjectManager = binaryObjectManager;
        }

        #region Get Settings

        public async Task<TenantSettingsEditDto> GetAllSettings()
        {
            var settings = new TenantSettingsEditDto
            {
                UserManagement = await GetUserManagementSettingsAsync(),
                Ui             = await GetUiSettingsAsync(),
                Company        = await GetCompanySettingsAsync(),
                Finance        = await GetFinanceSettingsAsync(),
                Print          = await GetPrintSettingsAsync(),
            };

            if (!_multiTenancyConfig.IsEnabled || Clock.SupportsMultipleTimezone)
            {
                settings.General = await GetGeneralSettingsAsync();
            }

            return settings;
        }

        private async Task<GeneralSettingsEditDto> GetGeneralSettingsAsync()
        {
            var settings = new GeneralSettingsEditDto();

            if (Clock.SupportsMultipleTimezone)
            {
                var timezone =
                    await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone,
                        AbpSession.GetTenantId());

                settings.Timezone = timezone;
                settings.TimezoneForComparison = timezone;
            }

            var defaultTimeZoneId =
                await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Tenant, AbpSession.TenantId);

            if (settings.Timezone == defaultTimeZoneId)
            {
                settings.Timezone = string.Empty;
            }

            return settings;
        }

        private async Task<CompanySettingsEditDto> GetCompanySettingsAsync()
        {
            var logoIdStr = await SettingManager.GetSettingValueAsync(AppSettings.Company.LogoId);
            Guid? logoId = null;
            if (Guid.TryParse(logoIdStr, out var parsedId))
            {
                logoId = parsedId;
            }

            return new CompanySettingsEditDto
            {
                Name        = await SettingManager.GetSettingValueAsync(AppSettings.Company.Name),
                Phone       = await SettingManager.GetSettingValueAsync(AppSettings.Company.Phone),
                Email       = await SettingManager.GetSettingValueAsync(AppSettings.Company.Email),
                Address     = await SettingManager.GetSettingValueAsync(AppSettings.Company.Address),
                TaxNumber   = await SettingManager.GetSettingValueAsync(AppSettings.Company.TaxNumber),
                Website     = await SettingManager.GetSettingValueAsync(AppSettings.Company.Website),
                FormStateId = await SettingManager.GetSettingValueAsync(AppSettings.Company.FormStateId),
                LogoId      = logoId,
            };
        }

        private async Task<FinanceSettingsEditDto> GetFinanceSettingsAsync()
        {
            return new FinanceSettingsEditDto
            {
                Currency               = await SettingManager.GetSettingValueAsync(AppSettings.Finance.Currency),
                DateFormat             = await SettingManager.GetSettingValueAsync(AppSettings.Finance.DateFormat),
                FiscalYearStart        = await SettingManager.GetSettingValueAsync(AppSettings.Finance.FiscalYearStart),
                AutoFillReceiptBalance = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Finance.AutoFillReceiptBalance),
            };
        }

        private async Task<UiSettingsEditDto> GetUiSettingsAsync()
        {
            return new UiSettingsEditDto
            {
                SearchActive = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UiManagement.SearchActive)
            };
        }

        private async Task<TenantUserManagementSettingsEditDto> GetUserManagementSettingsAsync()
        {
            return new TenantUserManagementSettingsEditDto
            {
                AllowSelfRegistration =
                    await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowSelfRegistration),

            };
        }

        private async Task<PrintSettingsEditDto> GetPrintSettingsAsync()
        {
            var json = await SettingManager.GetSettingValueAsync(AppSettings.Print.Settings);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new PrintSettingsEditDto();
            }

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<PrintSettingsEditDto>(json) ?? new PrintSettingsEditDto();
            }
            catch
            {
                return new PrintSettingsEditDto();
            }
        }

        #endregion

        #region Update Settings

        public async Task UpdateAllSettings(TenantSettingsEditDto input)
        {
            await UpdateUserManagementSettingsAsync(input.UserManagement);
            await UpdateUiSettingsAsync(input.Ui);

            if (input.Company != null)
                await UpdateCompanySettingsAsync(input.Company);

            if (input.Finance != null)
                await UpdateFinanceSettingsAsync(input.Finance);

            if (input.Print != null)
                await UpdatePrintSettingsAsync(input.Print);

            if (input.General != null)
            {
                //Time Zone
                if (Clock.SupportsMultipleTimezone)
                {
                    if (input.General.Timezone.IsNullOrEmpty())
                    {
                        var defaultValue =
                           await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Tenant, AbpSession.TenantId);
                        await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(),
                            TimingSettingNames.TimeZone, defaultValue);
                    }
                    else
                    {
                        _timeZoneService.ValidateTimezone(input.General.Timezone);
                        await SettingManager.ChangeSettingForTenantAsync(AbpSession.GetTenantId(),
                            TimingSettingNames.TimeZone, input.General.Timezone);
                    }
                }
            }

            if (!_multiTenancyConfig.IsEnabled)
            {
                input.ValidateHostSettings();
            }
        }

        private async Task UpdateCompanySettingsAsync(CompanySettingsEditDto s)
        {
            var tid = AbpSession.GetTenantId();
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Company.Name,        s.Name        ?? "");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Company.Phone,       s.Phone       ?? "");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Company.Email,       s.Email       ?? "");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Company.Address,     s.Address     ?? "");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Company.TaxNumber,   s.TaxNumber   ?? "");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Company.Website,     s.Website     ?? "");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Company.FormStateId, s.FormStateId ?? "");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Company.LogoId,      s.LogoId?.ToString() ?? "");
        }

        private async Task UpdateFinanceSettingsAsync(FinanceSettingsEditDto s)
        {
            var tid = AbpSession.GetTenantId();
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Finance.Currency,               s.Currency        ?? "INR");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Finance.DateFormat,             s.DateFormat      ?? "DD/MM/YYYY");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Finance.FiscalYearStart,        s.FiscalYearStart ?? "04-01");
            await SettingManager.ChangeSettingForTenantAsync(tid, AppSettings.Finance.AutoFillReceiptBalance, s.AutoFillReceiptBalance.ToString().ToLowerInvariant());
        }

        private async Task UpdateUiSettingsAsync(UiSettingsEditDto settings)
        {
            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UiManagement.SearchActive,
                settings.SearchActive.ToString().ToLowerInvariant()
            );
        }


        private async Task UpdateUserManagementSettingsAsync(TenantUserManagementSettingsEditDto settings)
        {
            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.UserManagement.AllowSelfRegistration,
                settings.AllowSelfRegistration.ToString().ToLowerInvariant()
            );

        }

        private async Task UpdatePrintSettingsAsync(PrintSettingsEditDto settings)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
            await SettingManager.ChangeSettingForTenantAsync(
                AbpSession.GetTenantId(),
                AppSettings.Print.Settings,
                json
            );
        }

        #endregion

        #region Company Logo

        public async Task<UploadCompanyLogoOutput> UploadCompanyLogo(UploadCompanyLogoInput input)
        {
            if (string.IsNullOrEmpty(input?.FileBase64))
            {
                throw new UserFriendlyException("Please select a file to upload.");
            }

            byte[] logoBytes;
            try
            {
                logoBytes = Convert.FromBase64String(input.FileBase64);
            }
            catch
            {
                throw new UserFriendlyException("Invalid file data.");
            }

            const int maxLogoBytes = 500 * 1024; // 500 KB
            if (logoBytes.Length > maxLogoBytes)
            {
                throw new UserFriendlyException($"Logo file size cannot exceed {maxLogoBytes / 1024} KB.");
            }

            var tenantId = AbpSession.GetTenantId();

            // Delete old logo if exists
            var oldLogoIdStr = await SettingManager.GetSettingValueForTenantAsync(AppSettings.Company.LogoId, tenantId);
            if (Guid.TryParse(oldLogoIdStr, out var oldLogoId))
            {
                await _binaryObjectManager.DeleteAsync(oldLogoId);
            }

            // Save new logo to AppBinaryObjects
            var binaryObject = new BinaryObject(tenantId, logoBytes, $"CompanyLogo_{input.FileName}");
            await _binaryObjectManager.SaveAsync(binaryObject);

            // Update setting with new logo id
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Company.LogoId, binaryObject.Id.ToString());

            return new UploadCompanyLogoOutput { LogoId = binaryObject.Id };
        }

        public async Task DeleteCompanyLogo()
        {
            var tenantId = AbpSession.GetTenantId();
            var logoIdStr = await SettingManager.GetSettingValueForTenantAsync(AppSettings.Company.LogoId, tenantId);

            if (Guid.TryParse(logoIdStr, out var logoId))
            {
                await _binaryObjectManager.DeleteAsync(logoId);
                await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Company.LogoId, "");
            }
        }

        public async Task<GetCompanyLogoOutput> GetCompanyLogo()
        {
            var tenantId = AbpSession.GetTenantId();
            var logoIdStr = await SettingManager.GetSettingValueForTenantAsync(AppSettings.Company.LogoId, tenantId);

            if (!Guid.TryParse(logoIdStr, out var logoId))
            {
                return new GetCompanyLogoOutput { LogoId = null };
            }

            var binaryObject = await _binaryObjectManager.GetOrNullAsync(logoId);
            if (binaryObject == null)
            {
                return new GetCompanyLogoOutput { LogoId = null };
            }

            return new GetCompanyLogoOutput
            {
                LogoId = logoId,
                FileBase64 = Convert.ToBase64String(binaryObject.Bytes),
                FileName = binaryObject.Description
            };
        }

        /// <summary>
        /// Get the current user's quick actions setting (JSON string).
        /// No admin permission required — any authenticated user can read their own.
        /// </summary>
        [AbpAuthorize]
        public async Task<string> GetQuickActions()
        {
            var json = await SettingManager.GetSettingValueAsync(AppSettings.DashboardCustomization.QuickActions);
            return json ?? "{\"hidden\":[],\"custom\":[]}";
        }

        /// <summary>
        /// Save the current user's quick actions setting (JSON string).
        /// </summary>
        [AbpAuthorize]
        public async Task SaveQuickActions(SaveQuickActionsInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                AppSettings.DashboardCustomization.QuickActions,
                input.Json
            );
        }

        #endregion
    }

    public class SaveQuickActionsInput
    {
        public string Json { get; set; }
    }
}