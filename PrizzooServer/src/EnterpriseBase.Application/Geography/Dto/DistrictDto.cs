using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Geography.Dto
{
    public class DistrictDto : EntityDto<int>
    {
        [Required]
        [MaxLength(100)]
        public string DistrictName { get; set; }

        [Required]
        public int StateId { get; set; }

        public string StateName { get; set; }
        public string CountryName { get; set; }
    }

    public class CreateDistrictDto
    {
        [Required]
        [MaxLength(100)]
        public string DistrictName { get; set; }

        [Required]
        public int StateId { get; set; }
    }

    public class UpdateDistrictDto : EntityDto<int>
    {
        [Required]
        [MaxLength(100)]
        public string DistrictName { get; set; }

        [Required]
        public int StateId { get; set; }
    }
}