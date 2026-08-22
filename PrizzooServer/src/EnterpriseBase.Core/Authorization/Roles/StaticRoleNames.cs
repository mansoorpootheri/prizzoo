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

        /// <summary>OTP-verified shopper - granted on first successful OTP login, see OtpAuthController.</summary>
        public const string Shopper = "Shopper";
    }
}
