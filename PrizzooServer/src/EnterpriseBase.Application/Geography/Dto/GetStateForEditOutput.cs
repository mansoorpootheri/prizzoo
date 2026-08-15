namespace EnterpriseBase.Geography.Dto
{
    public class GetStateForEditOutput
    {
        public CreateStateEditDto State { get; set; }
        public string CountryName { get; set; }
    }

    public class CreateStateEditDto
    {
        public int Id { get; set; }
        public string StateName { get; set; }
        public string StateCode { get; set; }
        public int CountryId { get; set; }
    }
}