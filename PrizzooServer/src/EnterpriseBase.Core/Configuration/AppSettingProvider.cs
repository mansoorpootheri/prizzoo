using Abp.Configuration;
using Abp.Json;
using Abp.Net.Mail;
using EnterpriseBase.DashboardCustomization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using static EnterpriseBase.EnterpriseBaseDashboardCustomizationConsts;

namespace EnterpriseBase.Configuration;

public class AppSettingProvider : SettingProvider
{
    private readonly IConfigurationRoot _appConfiguration;
    VisibleSettingClientVisibilityProvider _visibleSettingClientVisibilityProvider;

    public AppSettingProvider(IAppConfigurationAccessor configurationAccessor)
    {
        _appConfiguration = configurationAccessor.Configuration;
        _visibleSettingClientVisibilityProvider = new VisibleSettingClientVisibilityProvider();
    }
    public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
    {

        return GetHostSettings().Union(GetTenantSettings())
            // theme settings
            .Union(GetDefaultThemeSettings())
            .Union(GetTheme2Settings())
            .Union(GetTheme3Settings())
            .Union(GetTheme4Settings())
            .Union(GetTheme5Settings())
            .Union(GetTheme6Settings())
            .Union(GetTheme7Settings())
            .Union(GetTheme8Settings())
            .Union(GetTheme9Settings())
            .Union(GetTheme10Settings())
            .Union(GetTheme11Settings())
            .Union(GetTheme12Settings())
            .Union(GetTheme13Settings())
            .Union(GetDashboardSettings());
    }

    private IEnumerable<SettingDefinition> GetDashboardSettings()
    {
        var mvcDefaultHostView = GetDefaultMvcHostDashboardView();
        var mvcDefaultTenantView = GetDefaultMvcTenantDashboardView();

        var angularDefaultHostView = GetDefaultAngularHostDashboardView();
        var angularDefaultTenantView = GetDefaultAngularTenantDashboardView();

        string GetSettingName(string application, string dashboardName)
        {
            return AppSettings.DashboardCustomization.Configuration + "." + application + "." + dashboardName;
        }

        return new[]
        {
                new SettingDefinition(
                    GetSettingName(
                        EnterpriseBaseDashboardCustomizationConsts.Applications.Mvc,
                        mvcDefaultHostView.DashboardName
                    ),
                    mvcDefaultHostView.ToJsonString(),
                    scopes: SettingScopes.All,
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider
                ),
                new SettingDefinition(
                    GetSettingName(
                        EnterpriseBaseDashboardCustomizationConsts.Applications.Mvc,
                        mvcDefaultTenantView.DashboardName
                    ),
                    mvcDefaultTenantView.ToJsonString(),
                    scopes: SettingScopes.All,
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider
                ),
                new SettingDefinition(
                    GetSettingName(
                        EnterpriseBaseDashboardCustomizationConsts.Applications.Angular,
                        angularDefaultHostView.DashboardName
                    ),
                    angularDefaultHostView.ToJsonString(),
                    scopes: SettingScopes.All,
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider
                ),
                new SettingDefinition(
                    GetSettingName(
                        EnterpriseBaseDashboardCustomizationConsts.Applications.Angular,
                        angularDefaultTenantView.DashboardName
                    ),
                    angularDefaultTenantView.ToJsonString(),
                    scopes: SettingScopes.All,
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider
                )
            };
    }

    private IEnumerable<SettingDefinition> GetTenantSettings()
    {
        return new[]
        {
                new SettingDefinition(AppSettings.UserManagement.AllowSelfRegistration,
                    GetFromAppSettings(AppSettings.UserManagement.AllowSelfRegistration, "true"),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),

                // Company
                new SettingDefinition(AppSettings.Company.Name,
                    GetFromAppSettings(AppSettings.Company.Name, ""),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Company.Phone,
                    GetFromAppSettings(AppSettings.Company.Phone, ""),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Company.Email,
                    GetFromAppSettings(AppSettings.Company.Email, ""),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Company.Address,
                    GetFromAppSettings(AppSettings.Company.Address, ""),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Company.TaxNumber,
                    GetFromAppSettings(AppSettings.Company.TaxNumber, ""),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Company.Website,
                    GetFromAppSettings(AppSettings.Company.Website, ""),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Company.FormStateId,
                    GetFromAppSettings(AppSettings.Company.FormStateId, ""),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Company.LogoId,
                    GetFromAppSettings(AppSettings.Company.LogoId, ""),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),

                // Finance
                new SettingDefinition(AppSettings.Finance.Currency,
                    GetFromAppSettings(AppSettings.Finance.Currency, "INR"),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Finance.DateFormat,
                    GetFromAppSettings(AppSettings.Finance.DateFormat, "DD/MM/YYYY"),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Finance.FiscalYearStart,
                    GetFromAppSettings(AppSettings.Finance.FiscalYearStart, "04-01"),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.Finance.AutoFillReceiptBalance,
                    GetFromAppSettings(AppSettings.Finance.AutoFillReceiptBalance, "true"),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),

                // Print
                new SettingDefinition(AppSettings.Print.Settings,
                    GetFromAppSettings(AppSettings.Print.Settings, ""),
                    scopes: SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
            };
    }
    public Dashboard GetDefaultMvcHostDashboardView()
    {
        //It is the default dashboard view which your user will see if they don't do any customization.
        return new Dashboard
        {
            DashboardName = EnterpriseBaseDashboardCustomizationConsts.DashboardNames.DefaultHostDashboard,
            Pages = new List<Page>
                {
                    new Page($"Page_{EnterpriseBaseDashboardCustomizationConsts.DashboardNames.DefaultHostDashboard}_{EnterpriseBaseDashboardCustomizationConsts.DefaultPageUniqueName}_{1}")
                    {
                        Name = EnterpriseBaseDashboardCustomizationConsts.DefaultPageName,
                        Widgets = new List<Widget>
                        {
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                    .TopStats, // Top Stats
                                Height = 6,
                                Width = 12,
                                PositionX = 0,
                                PositionY = 0
                            },
                            new Widget
                            {
                                WidgetId =
                                    EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                        .IncomeStatistics, // Income Statistics
                                Height = 11,
                                Width = 7,
                                PositionX = 0,
                                PositionY = 6
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                    .RecentTenants, // Recent tenants
                                Height = 10,
                                Width = 5,
                                PositionX = 7,
                                PositionY = 17
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                    .SubscriptionExpiringTenants, // Subscription expiring tenants
                                Height = 10,
                                Width = 7,
                                PositionX = 0,
                                PositionY = 17
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                    .EditionStatistics, // Edition statistics
                                Height = 11,
                                Width = 5,
                                PositionX = 7,
                                PositionY = 6
                            }
                        }
                    }
                }
        };
    }

    public Dashboard GetDefaultMvcTenantDashboardView()
    {
        // It is the default dashboard view which your user will see if they don't do any customization.
        return new Dashboard
        {
            DashboardName = EnterpriseBaseDashboardCustomizationConsts.DashboardNames.DefaultTenantDashboard,
            Pages = new List<Page>
                {
                    new Page($"Page_{EnterpriseBaseDashboardCustomizationConsts.DashboardNames.DefaultTenantDashboard}_{EnterpriseBaseDashboardCustomizationConsts.DefaultPageUniqueName}_{1}")
                    {
                        Name = EnterpriseBaseDashboardCustomizationConsts.DefaultPageName,
                        Widgets = new List<Widget>
                        {
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .GeneralStats, // General Stats
                                Height = 9,
                                Width = 6,
                                PositionX = 0,
                                PositionY = 19
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .ProfitShare, // Profit Share
                                Height = 13,
                                Width = 6,
                                PositionX = 0,
                                PositionY = 28
                            },
                            new Widget
                            {
                                WidgetId =
                                    EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                        .MemberActivity, // Memeber Activity
                                Height = 13,
                                Width = 6,
                                PositionX = 6,
                                PositionY = 28
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .RegionalStats, // Regional Stats
                                Height = 14,
                                Width = 6,
                                PositionX = 6,
                                PositionY = 5
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .DailySales, // Daily Sales
                                Height = 9,
                                Width = 6,
                                PositionX = 6,
                                PositionY = 19
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .TopStats, // Top Stats
                                Height = 5,
                                Width = 12,
                                PositionX = 0,
                                PositionY = 0
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .SalesSummary, // Sales Summary
                                Height = 14,
                                Width = 6,
                                PositionX = 0,
                                PositionY = 5
                            }
                        }
                    }
                }
        };
    }

    public Dashboard GetDefaultAngularHostDashboardView()
    {
        // It is the default dashboard view which your user will see if they don't do any customization.
        return new Dashboard
        {
            DashboardName = EnterpriseBaseDashboardCustomizationConsts.DashboardNames.DefaultHostDashboard,
            Pages = new List<Page>
                {
                    new Page($"Page_{EnterpriseBaseDashboardCustomizationConsts.DashboardNames.DefaultHostDashboard}_{EnterpriseBaseDashboardCustomizationConsts.DefaultPageUniqueName}_{1}")
                    {
                        Name = EnterpriseBaseDashboardCustomizationConsts.DefaultPageName,
                        Widgets = new List<Widget>
                        {
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                    .TopStats, // Top Stats
                                Height = 5,
                                Width = 12,
                                PositionX = 0,
                                PositionY = 0
                            },
                            new Widget
                            {
                                WidgetId =
                                    EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                        .IncomeStatistics, // Income Statistics
                                Height = 8,
                                Width = 7,
                                PositionX = 0,
                                PositionY = 5
                            },
                            new Widget
                            {
                                WidgetId =
                                    EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                        .RecentTenants, // Recent tenants
                                Height = 9,
                                Width = 5,
                                PositionX = 7,
                                PositionY = 13
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                    .SubscriptionExpiringTenants, // Subscription expiring tenants
                                Height = 9,
                                Width = 7,
                                PositionX = 0,
                                PositionY = 13
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Host
                                    .EditionStatistics, // Edition statistics
                                Height = 8,
                                Width = 5,
                                PositionX = 7,
                                PositionY = 5
                            }
                        }
                    }
                }
        };
    }

    public Dashboard GetDefaultAngularTenantDashboardView()
    {
        //It is the default dashboard view which your user will see if they don't do any customization.
        return new Dashboard
        {
            DashboardName = EnterpriseBaseDashboardCustomizationConsts.DashboardNames.DefaultTenantDashboard,
            Pages = new List<Page>
                {
                    new Page($"Page_{EnterpriseBaseDashboardCustomizationConsts.DashboardNames.DefaultTenantDashboard}_{EnterpriseBaseDashboardCustomizationConsts.DefaultPageUniqueName}_{1}")
                    {
                        Name = EnterpriseBaseDashboardCustomizationConsts.DefaultPageName,
                        Widgets = new List<Widget>
                        {
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .TopStats, // Top Stats
                                Height = 4,
                                Width = 12,
                                PositionX = 0,
                                PositionY = 0
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .SalesSummary, // Sales Summary
                                Height = 12,
                                Width = 6,
                                PositionX = 0,
                                PositionY = 4
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .RegionalStats, // Regional Stats
                                Height = 12,
                                Width = 6,
                                PositionX = 6,
                                PositionY = 4
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .GeneralStats, // General Stats
                                Height = 8,
                                Width = 6,
                                PositionX = 0,
                                PositionY = 16
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .DailySales, // Daily Sales
                                Height = 8,
                                Width = 6,
                                PositionX = 6,
                                PositionY = 16
                            },
                            new Widget
                            {
                                WidgetId = EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                    .ProfitShare, // Profit Share
                                Height = 11,
                                Width = 6,
                                PositionX = 0,
                                PositionY = 24
                            },
                            new Widget
                            {
                                WidgetId =
                                    EnterpriseBaseDashboardCustomizationConsts.Widgets.Tenant
                                        .MemberActivity, // Member Activity
                                Height = 11,
                                Width = 6,
                                PositionX = 6,
                                PositionY = 24
                            }
                        }
                    }
                }
        };
    }
    private string GetFromAppSettings(string name, string defaultValue = null)
    {
        return GetFromSettings("App:" + name, defaultValue);
    }
    private string GetFromSettings(string name, string defaultValue = null)
    {
        return _appConfiguration[name] ?? defaultValue;
    }

    private IEnumerable<SettingDefinition> GetHostSettings()
    {
        return new[]
        {
                new SettingDefinition(AppSettings.TenantManagement.AllowSelfRegistration,
                    GetFromAppSettings(AppSettings.TenantManagement.AllowSelfRegistration, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.UiManagement.Theme,
                    GetFromAppSettings(AppSettings.UiManagement.Theme, "default"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider,
                    scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider,
                    scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetDefaultThemeSettings()
    {
        var themeName = "default";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.Skin,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.Skin, "light"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SubHeader.Style,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SubHeader.Style, "solid"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LeftAside.AsideSkin,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LeftAside.AsideSkin, "light"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LeftAside.FixedAside,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LeftAside.FixedAside, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LeftAside.AllowAsideMinimizing,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LeftAside.AllowAsideMinimizing,
                        "true"), clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LeftAside.DefaultMinimizedAside,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LeftAside.DefaultMinimizedAside,
                        "false"), clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LeftAside.SubmenuToggle,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LeftAside.SubmenuToggle, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LeftAside.HoverableAside,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LeftAside.HoverableAside,
                        "true"), clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),


                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.DesktopFixedFooter,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.DesktopFixedFooter, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.MobileFixedFooter,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.MobileFixedFooter, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Toolbar.DesktopFixedToolbar,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Toolbar.DesktopFixedToolbar, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Toolbar.MobileFixedToolbar,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Toolbar.MobileFixedToolbar, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme2Settings()
    {
        var themeName = "theme2";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MinimizeType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MinimizeType, "topbar"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
            };
    }

    private IEnumerable<SettingDefinition> GetTheme3Settings()
    {
        var themeName = "theme3";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SubHeader.Style,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SubHeader.Style, "solid"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme4Settings()
    {
        var themeName = "theme4";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fluid"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MinimizeType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MinimizeType, "menu"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),


                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme5Settings()
    {
        var themeName = "theme5";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MinimizeType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MinimizeType, "menu"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.DesktopFixedFooter,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.DesktopFixedFooter, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme6Settings()
    {
        var themeName = "theme6";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SubHeader.Style,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SubHeader.Style, "solid"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme7Settings()
    {
        var themeName = "theme7";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SubHeader.Style,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SubHeader.Style, "solid"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.DesktopFixedFooter,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.DesktopFixedFooter, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.MobileFixedFooter,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.MobileFixedFooter, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme8Settings()
    {
        var themeName = "theme8";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fluid"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme9Settings()
    {
        var themeName = "theme9";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme10Settings()
    {
        var themeName = "theme10";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fluid"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.DesktopFixedHeader, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme11Settings()
    {
        var themeName = "theme11";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fluid-xxl"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LeftAside.FixedAside,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LeftAside.FixedAside, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme12Settings()
    {
        var themeName = "theme12";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fluid-xxl"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LeftAside.FixedAside,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LeftAside.FixedAside, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)
            };
    }

    private IEnumerable<SettingDefinition> GetTheme13Settings()
    {
        var themeName = "theme13";

        return new[]
        {
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.DarkMode,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.DarkMode, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LayoutType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LayoutType, "fluid"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Header.MobileFixedHeader, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.LeftAside.FixedAside,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.LeftAside.FixedAside, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.Footer.FooterWidthType, "fixed"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),
                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SearchActive,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SearchActive, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All),

                new SettingDefinition(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed,
                    GetFromAppSettings(themeName + "." + AppSettings.UiManagement.SubHeader.Fixed, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.All)

            };
    }




}
