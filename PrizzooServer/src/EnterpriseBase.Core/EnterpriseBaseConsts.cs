using EnterpriseBase.Debugging;

namespace EnterpriseBase;

public class EnterpriseBaseConsts
{
    public const string LocalizationSourceName = "EnterpriseBase";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = true;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "140640f3b8a846d9965c8f9eabd968bd";

    public const bool AllowTenantsToChangeEmailSettings = false;

    /// <summary>
    /// TODO: change this to your real phone number before running -
    /// TenantRoleAndUserBuilder seeds the initial tenant Admin account with
    /// this as its UserName/PhoneNumber, and admin login is phone+OTP only
    /// (see OtpAuthController) - there is no username/password fallback for
    /// this account. The OTP code itself is still hardcoded to "123456"
    /// (see OtpChallengeService), same as every other phone number, until a
    /// real SMS gateway is wired up.
    /// </summary>
    public const string InitialAdminPhoneNumber = "9847055857";
}
