using System;

namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class CompanySettingsEditDto
    {
        public string Name        { get; set; }
        public string Phone       { get; set; }
        public string Email       { get; set; }
        public string Address     { get; set; }
        public string TaxNumber   { get; set; }
        public string Website     { get; set; }
        public string FormStateId { get; set; }
        public Guid?  LogoId      { get; set; }
    }
}
