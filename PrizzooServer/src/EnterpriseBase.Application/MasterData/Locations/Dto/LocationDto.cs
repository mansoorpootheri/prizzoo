using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.MasterData.Locations.Dto
{
    public class LocationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEditLocationDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public int DistrictId { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
