using System;

namespace EnterpriseBase
{
    /// <summary>
    /// Some consts used in the application.
    /// </summary>
    public class AppConsts
    {
        /// <summary>
        /// Default page size for paged requests.
        /// </summary>
        public const int DefaultPageSize = 10;

        /// <summary>
        /// Maximum allowed page size for paged requests.
        /// </summary>
        public const int MaxPageSize = 1000;

        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public const string DefaultPassPhrase = "25372c984f51415e8879df7f132f88cd";

        public const int ResizedMaxProfilePictureBytesUserFriendlyValue = 1024;

        public const int MaxProfilePictureBytesUserFriendlyValue = 5;

        public const string TokenValidityKey = "token_validity_key";
        public const string RefreshTokenValidityKey = "refresh_token_validity_key";
        public const string SecurityStampKey = "AspNet.Identity.SecurityStamp";

        public const string TokenType = "token_type";

        public static string UserIdentifier = "user_identifier";

        public const string ThemeDefault = "default";
        public const string Theme2 = "theme2";
        public const string Theme3 = "theme3";
        public const string Theme4 = "theme4";
        public const string Theme5 = "theme5";
        public const string Theme6 = "theme6";
        public const string Theme7 = "theme7";
        public const string Theme8 = "theme8";
        public const string Theme9 = "theme9";
        public const string Theme10 = "theme10";
        public const string Theme11 = "theme11";
        public const string Theme12 = "theme12";
        public const string Theme13 = "theme13";

        public static TimeSpan AccessTokenExpiration = TimeSpan.FromDays(1);
        public static TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(365);

        /// <summary>
        /// Shopper OTP sessions deliberately outlive the 1-day Admin/ShopOwner
        /// token (see EnterpriseBaseWebCoreModule.ConfigureTokenAuth) - a
        /// shopper should stay logged in across app visits until they
        /// actually log out, not be forced to re-verify their phone number
        /// every day. There's no refresh-token flow for shoppers, so this
        /// TimeSpan is the JWT's entire lifetime.
        /// </summary>
        public static TimeSpan ShopperAccessTokenExpiration = TimeSpan.FromDays(30);
    }
}
