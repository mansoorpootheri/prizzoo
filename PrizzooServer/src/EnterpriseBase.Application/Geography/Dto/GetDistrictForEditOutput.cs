namespace EnterpriseBase.Geography.Dto
{
    public class GetDistrictForEditOutput
    {
        public CreateDistrictEditDto District { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
    }

    public class CreateDistrictEditDto
    {
        public int Id { get; set; }
        public string DistrictName { get; set; }
        public int StateId { get; set; }
    }
}