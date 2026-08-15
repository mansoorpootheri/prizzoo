namespace EnterpriseBase.Geography.Dto
{
    public class GetCountryForEditOutput
    {
        public CreateCountryEditDto Country { get; set; }
    }

    public class CreateCountryEditDto
    {
        public int Id { get; set; }
        public string CountryName { get; set; }
        public string IsoCode { get; set; }
        public string PhoneCode { get; set; }
    }
}