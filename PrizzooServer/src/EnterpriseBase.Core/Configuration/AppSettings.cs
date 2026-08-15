namespace EnterpriseBase.Configuration
{
    /// <summary>
    /// Defines string constants for setting names in the application.
    /// See <see cref="AppSettingProvider"/> for setting definitions.
    /// </summary>
    public static class AppSettings
    {

        public static class DashboardCustomization
        {
            public const string Configuration = "App.DashboardCustomization.Configuration";
            public const string QuickActions = "App.DashboardCustomization.QuickActions";
        }
        public static class HostManagement
        {
            
        }

        public static class TenantManagement
        {
            public const string AllowSelfRegistration = "App.TenantManagement.AllowSelfRegistration";
        }

        public static class UserManagement
        {
            public const string AllowSelfRegistration = "App.UserManagement.AllowSelfRegistration";
        }

        public static class Company
        {
            public const string Name        = "App.Company.Name";
            public const string Phone       = "App.Company.Phone";
            public const string Email       = "App.Company.Email";
            public const string Address     = "App.Company.Address";
            public const string TaxNumber   = "App.Company.TaxNumber";
            public const string Website     = "App.Company.Website";
            public const string FormStateId = "App.Company.FormStateId";
            public const string LogoId      = "App.Company.LogoId";
        }

        public static class Finance
        {
            public const string Currency                = "App.Finance.Currency";
            public const string DateFormat              = "App.Finance.DateFormat";
            public const string FiscalYearStart         = "App.Finance.FiscalYearStart";
            public const string AutoFillReceiptBalance  = "App.Finance.AutoFillReceiptBalance";
        }

        public static class Print
        {
            /// <summary>
            /// Stores the entire print settings as a JSON string.
            /// </summary>
            public const string Settings = "App.Print.Settings";
        }

        public static class UiManagement
        {
            public const string LayoutType = "App.UiManagement.LayoutType";
            public const string DarkMode = "App.UiManagement.DarkMode";
            public const string FixedBody = "App.UiManagement.Layout.FixedBody";
            public const string MobileFixedBody = "App.UiManagement.Layout.MobileFixedBody";

            public const string Theme = "App.UiManagement.Theme";

            public const string SearchActive = "App.UiManagement.MenuSearch";

            public static class Header
            {
                public const string DesktopFixedHeader = "App.UiManagement.Header.DesktopFixedHeader";
                public const string MobileFixedHeader = "App.UiManagement.Header.MobileFixedHeader";
                public const string Skin = "App.UiManagement.Header.Skin";
                public const string MinimizeType = "App.UiManagement.Header.MinimizeType";
                public const string MenuArrows = "App.UiManagement.Header.MenuArrows";
            }

            public static class SubHeader
            {
                public const string Fixed = "App.UiManagement.SubHeader.Fixed";
                public const string Style = "App.UiManagement.SubHeader.Style";
            }

            public static class LeftAside
            {
                public const string Position = "App.UiManagement.Left.Position";
                public const string AsideSkin = "App.UiManagement.Left.AsideSkin";
                public const string FixedAside = "App.UiManagement.Left.FixedAside";
                public const string AllowAsideMinimizing = "App.UiManagement.Left.AllowAsideMinimizing";
                public const string DefaultMinimizedAside = "App.UiManagement.Left.DefaultMinimizedAside";
                public const string HoverableAside = "App.UiManagement.Left.HoverableAside";
                public const string SubmenuToggle = "App.UiManagement.Left.SubmenuToggle";
            }

            public static class Footer
            {
                public const string DesktopFixedFooter = "App.UiManagement.Footer.DesktopFixedFooter";
                public const string MobileFixedFooter = "App.UiManagement.Footer.MobileFixedFooter";
                public const string FooterWidthType = "App.UiManagement.Footer.FooterWidthType";
            }

            public static class Toolbar
            {
                public const string DesktopFixedToolbar = "App.UiManagement.Toolbar.DesktopFixedToolbar";
                public const string MobileFixedToolbar = "App.UiManagement.Toolbar.MobileFixedToolbar";
            }
        }

    }
}
