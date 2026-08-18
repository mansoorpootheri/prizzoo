using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Models.Otp
{
    public class RequestOtpModel
    {
        [Required]
        [Phone]
        [MaxLength(30)]
        public string PhoneNumber { get; set; }
    }

    public class VerifyOtpModel
    {
        [Required]
        [Phone]
        [MaxLength(30)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(6)]
        public string Code { get; set; }
    }
}
