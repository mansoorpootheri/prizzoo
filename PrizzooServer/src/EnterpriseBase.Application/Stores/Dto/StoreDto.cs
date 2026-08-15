using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.Stores.Dto
{
    public class StoreDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string OpeningHours { get; set; }
        public string CategoryTags { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public Guid? ImageId { get; set; }

        /// <summary>Populated only by the public nearby-search endpoint.</summary>
        public double? DistanceKm { get; set; }
    }

    public class CreateStoreDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [Phone]
        [MaxLength(30)]
        public string Phone { get; set; }

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        [MaxLength(500)]
        public string OpeningHours { get; set; }

        [MaxLength(200)]
        public string CategoryTags { get; set; }

        public Guid? ImageId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateStoreDto : EntityDto<Guid>
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [Phone]
        [MaxLength(30)]
        public string Phone { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        [MaxLength(500)]
        public string OpeningHours { get; set; }

        [MaxLength(200)]
        public string CategoryTags { get; set; }

        public Guid? ImageId { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
    }

    public class PagedStoreRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string City { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsVerified { get; set; }
    }
}
