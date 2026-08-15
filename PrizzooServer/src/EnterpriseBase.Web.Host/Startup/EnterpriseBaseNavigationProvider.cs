using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using EnterpriseBase.Authorization;

namespace EnterpriseBase.Web.Host.Startup
{
    public class EnterpriseBaseNavigationProvider : NavigationProvider
    {
        public override void SetNavigation(INavigationProviderContext context)
        {
            context.Manager.MainMenu

                // ── Dashboard ────────────────────────────────────────────
                .AddItem(new MenuItemDefinition(
                    PageNames.Home,
                    L("Dashboard"),
                    url: "",
                    icon: "fas fa-tachometer-alt",
                    requiresAuthentication: true,
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenant_Dashboard)
                ))

                // ── Host Dashboard ───────────────────────────────────────
                .AddItem(new MenuItemDefinition(
                    "HostDashboard",
                    L("Dashboard"),
                    url: "",
                    icon: "fas fa-tachometer-alt",
                    requiresAuthentication: true,
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Administration_Host_Dashboard)
                ))

                // ── Tenants ──────────────────────────────────────────────
                .AddItem(new MenuItemDefinition(
                    PageNames.Tenants,
                    L("Tenants"),
                    url: "Tenants",
                    icon: "fas fa-building",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
                ))

                // ── Editions ─────────────────────────────────────────────
                .AddItem(new MenuItemDefinition(
                    PageNames.Host.Editions,
                    L("Editions"),
                    url: "Editions",
                    icon: "fas fa-layer-group",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Editions)
                ))

                // ── Subscription Requests ────────────────────────────────
                .AddItem(new MenuItemDefinition(
                    "SubscriptionRequests",
                    L("SubscriptionRequests"),
                    url: "SubscriptionRequests",
                    icon: "fas fa-bell",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
                ))

                // ── Subscription ─────────────────────────────────────────
                .AddItem(new MenuItemDefinition(
                    PageNames.Host.Subscription,
                    L("Subscription"),
                    url: "Subscription",
                    icon: "fas fa-id-card",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Subscription)
                ))

                // ── Catalog ──────────────────────────────────────────────
                .AddItem(new MenuItemDefinition(
                    "Catalog",
                    L("Catalog"),
                    icon: "fas fa-boxes"
                )
                .AddItem(new MenuItemDefinition(
                    "Stores",
                    L("Stores"),
                    url: "Stores",
                    icon: "fas fa-store",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Stores)
                ))
                .AddItem(new MenuItemDefinition(
                    "Products",
                    L("Products"),
                    url: "Products",
                    icon: "fas fa-tag",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Products)
                ))
                .AddItem(new MenuItemDefinition(
                    "Categories",
                    L("Categories"),
                    url: "Categories",
                    icon: "fas fa-th-large",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Products_Categories)
                )))

                // ── Price Moderation ─────────────────────────────────────
                .AddItem(new MenuItemDefinition(
                    "PriceModeration",
                    L("PriceModeration"),
                    url: "PriceModeration",
                    icon: "fas fa-gavel",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_PriceModeration)
                ))

                // ── Administration ───────────────────────────────────────
                .AddItem(new MenuItemDefinition(
                    PageNames.Common.Administration,
                    L("Administration"),
                    icon: "fas fa-cogs"
                )
                .AddItem(new MenuItemDefinition(
                    PageNames.Tenant.Branch,
                    L("Branches"),
                    url: "Branches",
                    icon: "fas fa-sitemap",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Administration_Branch)
                ))
                .AddItem(new MenuItemDefinition(
                    PageNames.Tenant.Employee,
                    L("Employees"),
                    url: "Employees",
                    icon: "fas fa-id-badge",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Administration_Employee)
                ))
                .AddItem(new MenuItemDefinition(
                    PageNames.Users,
                    L("Users"),
                    url: "Users",
                    icon: "fas fa-users",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Users)
                ))
                .AddItem(new MenuItemDefinition(
                    PageNames.Roles,
                    L("Roles"),
                    url: "Roles",
                    icon: "fas fa-user-shield",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
                ))
                .AddItem(new MenuItemDefinition(
                    PageNames.Geography.Countries,
                    L("Countries"),
                    url: "Countries",
                    icon: "fas fa-flag",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Geography_Countries)
                ))
                .AddItem(new MenuItemDefinition(
                    PageNames.Geography.States,
                    L("States"),
                    url: "States",
                    icon: "fas fa-map",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Geography_States)
                ))
                .AddItem(new MenuItemDefinition(
                    PageNames.Geography.Districts,
                    L("Districts"),
                    url: "Districts",
                    icon: "fas fa-map-marker-alt",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Geography_Districts)
                ))
                .AddItem(new MenuItemDefinition(
                    "Taxes",
                    L("Tax"),
                    url: "Taxes",
                    icon: "fas fa-percent",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Administration_Taxes)
                ))
                .AddItem(new MenuItemDefinition(
                    PageNames.Host.Settings,
                    L("Settings"),
                    url: "HostSettings",
                    icon: "fas fa-cog",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Administration_Host_Settings)
                ))
                .AddItem(new MenuItemDefinition(
                    PageNames.Tenant.Settings,
                    L("Settings"),
                    url: "Settings",
                    icon: "fas fa-wrench",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Administration_Tenant_Settings)
                ))
                .AddItem(new MenuItemDefinition(
                    "ReleaseNotes",
                    L("ReleaseNotes"),
                    url: "ReleaseNotes",
                    icon: "fas fa-bullhorn",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Administration_Host_Settings)
                )));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, EnterpriseBaseConsts.LocalizationSourceName);
        }
    }
}
