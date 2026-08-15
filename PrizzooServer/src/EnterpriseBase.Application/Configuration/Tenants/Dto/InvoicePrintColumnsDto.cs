namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class InvoicePrintColumnsDto
    {
        public PrintColumnSettingDto PassengerName { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto TicketNo { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto BasicFare { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto Discount { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto Tax { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto ServiceCharge { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto Total { get; set; } = new() { Visible = true, Heading = "" };
    }
}
