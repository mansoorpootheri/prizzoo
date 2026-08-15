using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Configuration.Host.Dto
{
    public class BackupDatabaseInput
    {
        [Required]
        public string DatabaseName { get; set; }
    }
}
