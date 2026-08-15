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

        // Branches
        var branches = administration.CreateChildPermission(PermissionNames.Pages_Administration_Branch, L("Branches"), multiTenancySides: MultiTenancySides.Tenant);
        branches.CreateChildPermission(PermissionNames.Pages_Administration_Branch_View, L("ViewBranches"), multiTenancySides: MultiTenancySides.Tenant);
        branches.CreateChildPermission(PermissionNames.Pages_Administration_Branch_Create, L("CreateBranches"), multiTenancySides: MultiTenancySides.Tenant);
        branches.CreateChildPermission(PermissionNames.Pages_Administration_Branch_Edit, L("EditBranches"), multiTenancySides: MultiTenancySides.Tenant);
        branches.CreateChildPermission(PermissionNames.Pages_Administration_Branch_Delete, L("DeleteBranches"), multiTenancySides: MultiTenancySides.Tenant);

        // Employee Types
        var employeeTypes = administration.CreateChildPermission(PermissionNames.Pages_Administration_EmployeeType, L("EmployeeTypes"), multiTenancySides: MultiTenancySides.Tenant);
        employeeTypes.CreateChildPermission(PermissionNames.Pages_Administration_EmployeeType_Create, L("CreateEmployeeTypes"), multiTenancySides: MultiTenancySides.Tenant);
        employeeTypes.CreateChildPermission(PermissionNames.Pages_Administration_EmployeeType_Edit, L("EditEmployeeTypes"), multiTenancySides: MultiTenancySides.Tenant);
        employeeTypes.CreateChildPermission(PermissionNames.Pages_Administration_EmployeeType_Delete, L("DeleteEmployeeTypes"), multiTenancySides: MultiTenancySides.Tenant);

        // Employees
        var employees = administration.CreateChildPermission(PermissionNames.Pages_Administration_Employee, L("Employees"), multiTenancySides: MultiTenancySides.Tenant);
        employees.CreateChildPermission(PermissionNames.Pages_Administration_Employee_View, L("ViewEmployees"), multiTenancySides: MultiTenancySides.Tenant);
        employees.CreateChildPermission(PermissionNames.Pages_Administration_Employee_Create, L("CreateEmployees"), multiTenancySides: MultiTenancySides.Tenant);
        employees.CreateChildPermission(PermissionNames.Pages_Administration_Employee_Edit, L("EditEmployees"), multiTenancySides: MultiTenancySides.Tenant);
        employees.CreateChildPermission(PermissionNames.Pages_Administration_Employee_Delete, L("DeleteEmployees"), multiTenancySides: MultiTenancySides.Tenant);

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
        var products = context.CreatePermission(PermissionNames.Pages_Products, L("Products"), multiTenancySides: MultiTenancySides.Host);
        products.CreateChildPermission(PermissionNames.Pages_Products_Create, L("CreateProducts"), multiTenancySides: MultiTenancySides.Host);
        products.CreateChildPermission(PermissionNames.Pages_Products_Edit, L("EditProducts"), multiTenancySides: MultiTenancySides.Host);
        products.CreateChildPermission(PermissionNames.Pages_Products_Delete, L("DeleteProducts"), multiTenancySides: MultiTenancySides.Host);

        var categories = products.CreateChildPermission(PermissionNames.Pages_Products_Categories, L("Categories"), multiTenancySides: MultiTenancySides.Host);
        categories.CreateChildPermission(PermissionNames.Pages_Products_Categories_Create, L("CreateCategories"), multiTenancySides: MultiTenancySides.Host);
        categories.CreateChildPermission(PermissionNames.Pages_Products_Categories_Edit, L("EditCategories"), multiTenancySides: MultiTenancySides.Host);
        categories.CreateChildPermission(PermissionNames.Pages_Products_Categories_Delete, L("DeleteCategories"), multiTenancySides: MultiTenancySides.Host);

        var units = products.CreateChildPermission(PermissionNames.Pages_Products_Units, L("Units"), multiTenancySides: MultiTenancySides.Host);
        units.CreateChildPermission(PermissionNames.Pages_Products_Units_Create, L("CreateUnits"), multiTenancySides: MultiTenancySides.Host);
        units.CreateChildPermission(PermissionNames.Pages_Products_Units_Edit, L("EditUnits"), multiTenancySides: MultiTenancySides.Host);
        units.CreateChildPermission(PermissionNames.Pages_Products_Units_Delete, L("DeleteUnits"), multiTenancySides: MultiTenancySides.Host);

        // Prizzoo: Stores
        var stores = context.CreatePermission(PermissionNames.Pages_Stores, L("Stores"), multiTenancySides: MultiTenancySides.Host);
        stores.CreateChildPermission(PermissionNames.Pages_Stores_Create, L("CreateStores"), multiTenancySides: MultiTenancySides.Host);
        stores.CreateChildPermission(PermissionNames.Pages_Stores_Edit, L("EditStores"), multiTenancySides: MultiTenancySides.Host);
        stores.CreateChildPermission(PermissionNames.Pages_Stores_Delete, L("DeleteStores"), multiTenancySides: MultiTenancySides.Host);

        // Prizzoo: Price moderation
        context.CreatePermission(PermissionNames.Pages_PriceModeration, L("PriceModeration"), multiTenancySides: MultiTenancySides.Host);
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, EnterpriseBaseConsts.LocalizationSourceName);
    }
}
