using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Authorization.Otp
{
    /// <summary>
    /// A single phone-number OTP request/verify cycle for shopper login.
    /// The code itself is never stored - only a hash - so a DB leak doesn't
    /// expose live codes. See OtpChallengeService for generation/verification.
    /// </summary>
    [Table("OtpChallenges")]
    public class OtpChallenge : FullAuditedEntity<Guid>
    {
        [Required]
        [MaxLength(30)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(128)]
        public string CodeHash { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? ConsumedAt { get; set; }

        public int AttemptCount { get; set; }
    }
}
