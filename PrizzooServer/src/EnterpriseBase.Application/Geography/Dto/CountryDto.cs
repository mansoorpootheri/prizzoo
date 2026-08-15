using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Geography.Dto
{
    public class CountryDto : EntityDto<int>
    {
        [Required]
        [MaxLength(100)]
        public string CountryName { get; set; }

        [MaxLength(10)]
        public string IsoCode { get; set; }

        [MaxLength(10)]
        public string PhoneCode { get; set; }
    }

    public class CreateCountryDto
    {
        [Required]
        [MaxLength(100)]
        public string CountryName { get; set; }

        [MaxLength(10)]
        public string IsoCode { get; set; }

        [MaxLength(10)]
        public string PhoneCode { get; set; }
    }

    public class UpdateCountryDto : EntityDto<int>
    {
        [Required]
        [MaxLength(100)]
        public string CountryName { get; set; }

        [MaxLength(10)]
        public string IsoCode { get; set; }

        [MaxLength(10)]
        public string PhoneCode { get; set; }
    }
}