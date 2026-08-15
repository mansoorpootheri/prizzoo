namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class InvoicePrintMetadataDto
    {
        public PrintColumnSettingDto AirlineCode { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto Sector { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto FlightNo { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto Narration { get; set; } = new() { Visible = true, Heading = "" };
        public PrintColumnSettingDto TravelDate { get; set; } = new() { Visible = true, Heading = "" };
    }
}
