namespace EnterpriseBase.Authorization.Accounts.Dto;

public class TenantSignupOutput
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int? TenantId { get; set; }
    public long? UserId { get; set; }
    public bool CanLogin { get; set; }
    public string TenancyName { get; set; }
}