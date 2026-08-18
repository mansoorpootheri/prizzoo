namespace EnterpriseBase.Authorization.Roles;

public static class StaticRoleNames
{
    public static class Host
    {
        public const string Admin = "Admin";
    }

    public static class Tenants
    {
        public const string Admin = "Admin";

        /// <summary>Host-admin-created shop owner - see StoreAppService.CreateAsync.</summary>
        public const string ShopOwner = "ShopOwner";

        /// <summary>OTP-verified shopper - granted on first successful OTP login, see OtpAuthController.</summary>
        public const string Shopper = "Shopper";
    }
}
