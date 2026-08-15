namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class FinanceSettingsEditDto
    {
        public string Currency                { get; set; }
        public string DateFormat              { get; set; }
        public string FiscalYearStart         { get; set; }
        public bool   AutoFillReceiptBalance  { get; set; }
    }
}
