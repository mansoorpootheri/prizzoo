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

        // Nullable so historical rows created before this field (or before
        // it became mandatory here) can still be null - but [Required] on a
        // Nullable<T> does check HasValue, so every new create/edit call
        // must supply real coordinates captured via the client's "use my
        // current location" action. There is no manual-entry path anymore.
        [Required]
        public decimal? Latitude { get; set; }

        [Required]
        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
