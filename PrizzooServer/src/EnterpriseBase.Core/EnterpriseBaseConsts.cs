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
}
