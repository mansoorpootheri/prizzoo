namespace EnterpriseBase.Dto
{
    public class FileDto
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileToken { get; set; }
        public byte[] FileContent { get; set; }

        public FileDto()
        {
        }

        public FileDto(string fileName, string fileType)
        {
            FileName = fileName;
            FileType = fileType;
        }
    }
}