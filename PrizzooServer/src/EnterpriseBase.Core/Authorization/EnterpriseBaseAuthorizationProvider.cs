using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace EnterpriseBase.Authorization;

public class EnterpriseBaseAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        // Users
        var user = context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
        user.CreateChildPermission(PermissionNames.Pages_Users_Create, L("CreateUsers"));
        user.CreateChildPermission(PermissionNames.Pages_Users_Edit, L("EditUsers"));
        user.CreateChildPermission(PermissionNames.Pages_Users_Delete, L("DeleteUsers"));
        user.CreateChildPermission(PermissionNames.Pages_Administration_Users_Impersonation, L("LoginForUsers"));
        context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));

        // Roles
        var roles = context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
        roles.CreateChildPermission(PermissionNames.Pages_Roles_Create, L("CreateRoles"));
        roles.CreateChildPermission(PermissionNames.Pages_Roles_Edit, L("EditRoles"));
        roles.CreateChildPermission(PermissionNames.Pages_Roles_Delete, L("DeleteRoles"));

        // Tenants
        var tenants = context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
        tenants.CreateChildPermission(PermissionNames.Pages_Tenants_Impersonation, L("LoginForTenants"), multiTenancySides: MultiTenancySides.Host);
        tenants.CreateChildPermission(PermissionNames.Pages_Tenants_ChangeFeatures, L("ChangingFeatures"), multiTenancySides: MultiTenancySides.Host);

        // Administration
        var administration = context.CreatePermission(PermissionNames.Pages_Administration, L("Administration"));
        administration.CreateChildPermission(PermissionNames.Pages_Administration_Host_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Host);
        administration.CreateChildPermission(PermissionNames.Pages_Administration_Tenant_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Tenant);

        // Taxes
        var taxes = administration.CreateChildPermission(PermissionNames.Pages_Administration_Taxes, L("Taxes"), multiTenancySides: MultiTenancySides.Tenant);
        taxes.CreateChildPermission(PermissionNames.Pages_Administration_Taxes_Create, L("CreateTaxes"), multiTenancySides: MultiTenancySides.Tenant);
        taxes.CreateChildPermission(PermissionNames.Pages_Administration_Taxes_Edit, L("EditTaxes"), multiTenancySides: MultiTenancySides.Tenant);
        taxes.CreateChildPermission(PermissionNames.Pages_Administration_Taxes_Delete, L("DeleteTaxes"), multiTenancySides: MultiTenancySides.Tenant);

        // Geography
        var geography = context.CreatePermission(PermissionNames.Pages_Geography, L("Geography"), multiTenancySides: MultiTenancySides.Host);
        var countries = geography.CreateChildPermission(PermissionNames.Pages_Geography_Countries, L("Countries"), multiTenancySides: MultiTenancySides.Host);
        countries.CreateChildPermission(PermissionNames.Pages_Geography_Countries_Create, L("CreateCountries"), multiTenancySides: MultiTenancySides.Host);
        countries.CreateChildPermission(PermissionNames.Pages_Geography_Countries_Edit, L("EditCountries"), multiTenancySides: MultiTenancySides.Host);
        countries.CreateChildPermission(PermissionNames.Pages_Geography_Countries_Delete, L("DeleteCountries"), multiTenancySides: MultiTenancySides.Host);

        var states = geography.CreateChildPermission(PermissionNames.Pages_Geography_States, L("States"), multiTenancySides: MultiTenancySides.Host);
        states.CreateChildPermission(PermissionNames.Pages_Geography_States_Create, L("CreateStates"), multiTenancySides: MultiTenancySides.Host);
        states.CreateChildPermission(PermissionNames.Pages_Geography_States_Edit, L("EditStates"), multiTenancySides: MultiTenancySides.Host);
        states.CreateChildPermission(PermissionNames.Pages_Geography_States_Delete, L("DeleteStates"), multiTenancySides: MultiTenancySides.Host);

        var districts = geography.CreateChildPermission(PermissionNames.Pages_Geography_Districts, L("Districts"), multiTenancySides: MultiTenancySides.Host);
        districts.CreateChildPermission(PermissionNames.Pages_Geography_Districts_Create, L("CreateDistricts"), multiTenancySides: MultiTenancySides.Host);
        districts.CreateChildPermission(PermissionNames.Pages_Geography_Districts_Edit, L("EditDistricts"), multiTenancySides: MultiTenancySides.Host);
        districts.CreateChildPermission(PermissionNames.Pages_Geography_Districts_Delete, L("DeleteDistricts"), multiTenancySides: MultiTenancySides.Host);

        // Editions
        var editions = context.CreatePermission(PermissionNames.Pages_Editions, L("Editions"), multiTenancySides: MultiTenancySides.Host);
        editions.CreateChildPermission(PermissionNames.Pages_Editions_Create, L("CreateEditions"), multiTenancySides: MultiTenancySides.Host);
        editions.CreateChildPermission(PermissionNames.Pages_Editions_Edit, L("EditEditions"), multiTenancySides: MultiTenancySides.Host);
        editions.CreateChildPermission(PermissionNames.Pages_Editions_Delete, L("DeleteEditions"), multiTenancySides: MultiTenancySides.Host);

        // Subscription
        context.CreatePermission(PermissionNames.Pages_Subscription, L("Subscription"), multiTenancySides: MultiTenancySides.Tenant);

        // Dashboard
        context.CreatePermission(PermissionNames.Pages_Tenant_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Tenant);
        context.CreatePermission(PermissionNames.Pages_Administration_Host_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Host);

        // Prizzoo: Products / catalog
        // Every permission below is Host | Tenant - the single Admin
        // account (phone+OTP, tenant-side) is now the only real admin
        // identity this app's frontend uses, so it needs full CRUD, not
        // the old Host-only carve-outs from when a separate host admin
        // handled catalog master data.
        var products = context.CreatePermission(PermissionNames.Pages_Products, L("Products"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        products.CreateChildPermission(PermissionNames.Pages_Products_Create, L("CreateProducts"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        products.CreateChildPermission(PermissionNames.Pages_Products_Edit, L("EditProducts"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        products.CreateChildPermission(PermissionNames.Pages_Products_Delete, L("DeleteProducts"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);

        var categories = products.CreateChildPermission(PermissionNames.Pages_Products_Categories, L("Categories"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        categories.CreateChildPermission(PermissionNames.Pages_Products_Categories_Create, L("CreateCategories"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        categories.CreateChildPermission(PermissionNames.Pages_Products_Categories_Edit, L("EditCategories"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        categories.CreateChildPermission(PermissionNames.Pages_Products_Categories_Delete, L("DeleteCategories"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);

        var units = products.CreateChildPermission(PermissionNames.Pages_Products_Units, L("Units"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        units.CreateChildPermission(PermissionNames.Pages_Products_Units_Create, L("CreateUnits"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        units.CreateChildPermission(PermissionNames.Pages_Products_Units_Edit, L("EditUnits"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        units.CreateChildPermission(PermissionNames.Pages_Products_Units_Delete, L("DeleteUnits"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);

        // Prizzoo: Stores
        var stores = context.CreatePermission(PermissionNames.Pages_Stores, L("Stores"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        stores.CreateChildPermission(PermissionNames.Pages_Stores_Create, L("CreateStores"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        stores.CreateChildPermission(PermissionNames.Pages_Stores_Edit, L("EditStores"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        stores.CreateChildPermission(PermissionNames.Pages_Stores_Delete, L("DeleteStores"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);

        // Prizzoo: Location master data (District already has its own
        // Pages_Geography_Districts CRUD, seeded/registered below)
        var locations = context.CreatePermission(PermissionNames.Pages_Locations, L("Locations"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        locations.CreateChildPermission(PermissionNames.Pages_Locations_Create, L("CreateLocations"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        locations.CreateChildPermission(PermissionNames.Pages_Locations_Edit, L("EditLocations"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
        locations.CreateChildPermission(PermissionNames.Pages_Locations_Delete, L("DeleteLocations"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);

        // Prizzoo: Price moderation
        context.CreatePermission(PermissionNames.Pages_PriceModeration, L("PriceModeration"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);

        // Prizzoo: OTP-verified shopper browsing
        context.CreatePermission(PermissionNames.Pages_Shopper, L("Shopper"), multiTenancySides: MultiTenancySides.Tenant);

        // Prizzoo: admin account management (add further admin phone numbers)
        context.CreatePermission(PermissionNames.Pages_Admins, L("Admins"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);

        // Prizzoo: read-only list of shopper accounts registered via OTP login
        context.CreatePermission(PermissionNames.Pages_RegisteredUsers, L("RegisteredUsers"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, EnterpriseBaseConsts.LocalizationSourceName);
    }
}
