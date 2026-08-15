using System;

namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class GetCompanyLogoOutput
    {
        public Guid? LogoId { get; set; }
        public string FileBase64 { get; set; }
        public string FileName { get; set; }
    }
}
