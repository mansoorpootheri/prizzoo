using Abp.Auditing;
using Abp.Authorization.Users;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Authorization.Accounts.Dto;

public class TenantSignupInput
{
    [Required]
    [StringLength(AbpUserBase.MaxNameLength)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(AbpUserBase.MaxSurnameLength)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(AbpUserBase.MaxEmailAddressLength)]
    public string Email { get; set; }

    [Required]
    [StringLength(AbpUserBase.MaxPlainPasswordLength)]
    [DisableAuditing]
    public string Password { get; set; }

    [Required]
    [StringLength(150)]
    public string CompanyName { get; set; }
}