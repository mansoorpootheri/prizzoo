using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Models.TokenAuth
{
    public class ImpersonateAuthenticateModel
    {
        [Required]
        public string ImpersonationToken { get; set; }
    }
}
