namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class UploadCompanyLogoInput
    {
        /// <summary>
        /// Base64-encoded image data
        /// </summary>
        public string FileBase64 { get; set; }
        public string FileName { get; set; }
    }
}
