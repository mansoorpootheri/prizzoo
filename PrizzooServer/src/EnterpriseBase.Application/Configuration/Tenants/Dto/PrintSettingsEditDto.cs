namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class PrintSettingsEditDto
    {
        /// <summary>
        /// Document title for invoice print (default: "Tax Invoice")
        /// </summary>
        public string InvoiceTitle { get; set; } = "Tax Invoice";

        /// <summary>
        /// Document title for misc invoice print (default: "Misc Invoice")
        /// </summary>
        public string MiscInvoiceTitle { get; set; } = "Misc Invoice";

        /// <summary>
        /// Column visibility and heading overrides for Invoice print
        /// </summary>
        public InvoicePrintColumnsDto InvoiceColumns { get; set; } = new();

        /// <summary>
        /// Metadata field visibility and heading overrides for Invoice print
        /// </summary>
        public InvoicePrintMetadataDto InvoiceMetadata { get; set; } = new();

        /// <summary>
        /// Column visibility and heading overrides for Misc Invoice print
        /// </summary>
        public MiscInvoicePrintColumnsDto MiscInvoiceColumns { get; set; } = new();
    }
}
