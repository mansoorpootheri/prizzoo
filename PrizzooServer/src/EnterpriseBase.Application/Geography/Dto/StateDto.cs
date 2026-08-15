using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Geography.Dto
{
    public class StateDto : EntityDto<int>
    {
        [Required]
        [MaxLength(100)]
        public string StateName { get; set; }

        [MaxLength(10)]
        public string StateCode { get; set; }

        [Required]
        public int CountryId { get; set; }

        public string CountryName { get; set; }
    }

    public class CreateStateDto
    {
        [Required]
        [MaxLength(100)]
        public string StateName { get; set; }

        [MaxLength(10)]
        public string StateCode { get; set; }

        [Required]
        public int CountryId { get; set; }
    }

    public class UpdateStateDto : EntityDto<int>
    {
        [Required]
        [MaxLength(100)]
        public string StateName { get; set; }

        [MaxLength(10)]
        public string StateCode { get; set; }

        [Required]
        public int CountryId { get; set; }
    }
}