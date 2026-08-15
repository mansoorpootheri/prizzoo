namespace EnterpriseBase.MultiTenancy.Dto
{
    public class TenantUserLookupDto
    {
        public long Id { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string EmailAddress { get; set; }
    }
}
