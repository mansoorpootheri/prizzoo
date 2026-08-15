using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.MultiTenancy;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.MultiTenancy.Dto;

[AutoMapTo(typeof(Tenant))]
public class CreateTenantDto
{
    [Required]
    [StringLength(AbpTenantBase.MaxTenancyNameLength)]
    [RegularExpression(AbpTenantBase.TenancyNameRegex)]
    public string TenancyName { get; set; }

    [Required]
    [StringLength(AbpTenantBase.MaxNameLength)]
    public string Name { get; set; }

    [Required]
    [StringLength(AbpUserBase.MaxEmailAddressLength)]
    public string AdminEmailAddress { get; set; }

    [StringLength(AbpTenantBase.MaxConnectionStringLength)]
    public string ConnectionString { get; set; }

    public bool IsActive { get; set; }

    public int? EditionId { get; set; }

    [StringLength(150)]
    public string HeadOfficeName { get; set; }
}
