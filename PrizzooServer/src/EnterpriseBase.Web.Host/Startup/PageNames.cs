namespace EnterpriseBase.Web.Host.Startup
{

    public class PageNames
    {
        public const string Home = "Home";
        public const string About = "About";
        public const string Tenants = "Tenants";
        public const string Users = "Users";
        public const string Roles = "Roles";
        public const string Countries = "Countries";
        public const string States = "States";
        public const string Districts = "Districts";



        public static class Common
        {
            public const string Administration = "Administration";
        }

        public static class Host
        {
            public const string Settings = "Administration.Settings.Host";
            public const string Editions = "Editions";
            public const string Subscription = "Subscription";
        }
        public static class Tenant
        {
            public const string Settings = "Administration.Settings.Tenant";
            public const string Branch = "Administration.Branch.Tenant";
            public const string Center = "Administration.Center.Tenant";
            public const string EmployeeType = "Administration.EmployeeType.Tenant";
            public const string Employee = "Administration.Employee.Tenant";
        }

        public static class Geography
        {
            public const string Countries = "Geography.Countries";
            public const string States = "Geography.States";
            public const string Districts = "Geography.Districts";
        }

        public static class Accounting
        {
            public const string Main = "Accounting";
            public const string AccountGroups = "Accounting.AccountGroups";
            public const string Ledger = "Accounting.Ledger";
            public const string BankTransactions = "Accounting.BankTransactions";
        }

        public static class Parties
        {
            public const string Main = "Parties";
            public const string PartyList = "Parties.PartyList";
        }

        public static class FinancialYears
        {
            public const string Main = "FinancialYears";
            public const string FinancialYearList = "FinancialYears.FinancialYearList";
        }

        public static class Vouchers
        {
            public const string Main = "Vouchers";
            public const string PaymentVoucher = "Vouchers.PaymentVoucher";
            public const string JournalVoucher = "Vouchers.JournalVoucher";
        }

        public static class Invoicing
        {
            public const string Main = "Invoicing";
            public const string Invoices = "Invoicing.Invoices";
        }

        public static class Reports
        {
            public const string Main = "Reports";
            public const string BalanceSheet = "Reports.BalanceSheet";
            public const string ProfitLoss = "Reports.ProfitLoss";
            public const string Ledger = "Reports.Ledger";
        }
    }
}
