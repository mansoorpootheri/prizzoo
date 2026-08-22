namespace EnterpriseBase.Authorization
{
    public static class PermissionNames
    {
        // Core
        public const string Pages_Tenants = "Pages.Tenants";
        public const string Pages_Tenants_Impersonation = "Pages.Tenants.Impersonation";
        public const string Pages_Tenants_ChangeFeatures = "Pages.Tenants.ChangeFeatures";

        public const string Pages_Users = "Pages.Users";
        public const string Pages_Users_Create = "Pages.Users.Create";
        public const string Pages_Users_Edit = "Pages.Users.Edit";
        public const string Pages_Users_Delete = "Pages.Users.Delete";
        public const string Pages_Users_Activation = "Pages.Users.Activation";
        public const string Pages_Administration_Users_Impersonation = "Pages.Administration.Users.Impersonation";

        public const string Pages_Roles = "Pages.Roles";
        public const string Pages_Roles_Create = "Pages.Roles.Create";
        public const string Pages_Roles_Edit = "Pages.Roles.Edit";
        public const string Pages_Roles_Delete = "Pages.Roles.Delete";

        // Administration
        public const string Pages_Administration = "Pages.Administration";
        public const string Pages_Administration_Host_Settings = "Pages.Administration.Host.Settings";
        public const string Pages_Administration_Tenant_Settings = "Pages.Administration.Tenant.Settings";
        public const string Pages_Administration_Host_Dashboard = "Pages.Administration.Host.Dashboard";
        public const string Pages_Administration_AuditLogs = "Pages.Administration.AuditLogs";

        public const string Pages_Administration_Taxes = "Pages.Administration.Taxes";
        public const string Pages_Administration_Taxes_Create = "Pages.Administration.Taxes.Create";
        public const string Pages_Administration_Taxes_Edit = "Pages.Administration.Taxes.Edit";
        public const string Pages_Administration_Taxes_Delete = "Pages.Administration.Taxes.Delete";

        // Geography
        public const string Pages_Geography = "Pages.Geography";
        public const string Pages_Geography_Countries = "Pages.Geography.Countries";
        public const string Pages_Geography_Countries_Create = "Pages.Geography.Countries.Create";
        public const string Pages_Geography_Countries_Edit = "Pages.Geography.Countries.Edit";
        public const string Pages_Geography_Countries_Delete = "Pages.Geography.Countries.Delete";

        public const string Pages_Geography_States = "Pages.Geography.States";
        public const string Pages_Geography_States_Create = "Pages.Geography.States.Create";
        public const string Pages_Geography_States_Edit = "Pages.Geography.States.Edit";
        public const string Pages_Geography_States_Delete = "Pages.Geography.States.Delete";

        public const string Pages_Geography_Districts = "Pages.Geography.Districts";
        public const string Pages_Geography_Districts_Create = "Pages.Geography.Districts.Create";
        public const string Pages_Geography_Districts_Edit = "Pages.Geography.Districts.Edit";
        public const string Pages_Geography_Districts_Delete = "Pages.Geography.Districts.Delete";

        // Editions
        public const string Pages_Editions = "Pages.Editions";
        public const string Pages_Editions_Create = "Pages.Editions.Create";
        public const string Pages_Editions_Edit = "Pages.Editions.Edit";
        public const string Pages_Editions_Delete = "Pages.Editions.Delete";

        // Subscription
        public const string Pages_Subscription = "Pages.Subscription";

        // Dashboard
        public const string Pages_Tenant_Dashboard = "Pages.Tenant.Dashboard";

        // Prizzoo: Products / catalog master data
        public const string Pages_Products = "Pages.Products";
        public const string Pages_Products_Create = "Pages.Products.Create";
        public const string Pages_Products_Edit = "Pages.Products.Edit";
        public const string Pages_Products_Delete = "Pages.Products.Delete";

        public const string Pages_Products_Categories = "Pages.Products.Categories";
        public const string Pages_Products_Categories_Create = "Pages.Products.Categories.Create";
        public const string Pages_Products_Categories_Edit = "Pages.Products.Categories.Edit";
        public const string Pages_Products_Categories_Delete = "Pages.Products.Categories.Delete";

        public const string Pages_Products_Units = "Pages.Products.Units";
        public const string Pages_Products_Units_Create = "Pages.Products.Units.Create";
        public const string Pages_Products_Units_Edit = "Pages.Products.Units.Edit";
        public const string Pages_Products_Units_Delete = "Pages.Products.Units.Delete";

        // Prizzoo: Stores
        public const string Pages_Stores = "Pages.Stores";
        public const string Pages_Stores_Create = "Pages.Stores.Create";
        public const string Pages_Stores_Edit = "Pages.Stores.Edit";
        public const string Pages_Stores_Delete = "Pages.Stores.Delete";

        // Prizzoo: Location master data - a locality within a Geography
        // District, e.g. "Feroke" within the "Kozhikode" district. District
        // (Geography module, Pages_Geography_Districts) plays the "city"
        // role in the store-creation form; Country/State are fixed
        // constants for MVP (India/Kerala) with no separate master UI.
        public const string Pages_Locations = "Pages.Locations";
        public const string Pages_Locations_Create = "Pages.Locations.Create";
        public const string Pages_Locations_Edit = "Pages.Locations.Edit";
        public const string Pages_Locations_Delete = "Pages.Locations.Delete";

        // Prizzoo: Price moderation
        public const string Pages_PriceModeration = "Pages.PriceModeration";

        // Prizzoo: OTP-verified shopper browsing
        public const string Pages_Shopper = "Pages.Shopper";

        // Prizzoo: admin account management (add further admin phone numbers)
        public const string Pages_Admins = "Pages.Admins";

        // Prizzoo: read-only list of shopper accounts registered via OTP login
        public const string Pages_RegisteredUsers = "Pages.RegisteredUsers";
    }
}
