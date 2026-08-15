namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class MiscInvoicePrintColumnsDto
    {
        public PrintColumnSettingDto Particulars { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto Amount { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto ServiceCharge { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto Tax { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto Total { get; set; } = new() { Visible = true, Heading = "" };
    }
}
