using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.Stores.Dto
{
    public class StoreDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Address { get; set; }

        /// <summary>Denormalized display copy of District.DistrictName - see Store.cs's City field doc comment.</summary>
        public string City { get; set; }

        public Guid? LocationId { get; set; }
        public string LocationName { get; set; }

        /// <summary>Location's parent District id - lets an edit form preselect the same City -> Location cascade used at creation.</summary>
        public int? DistrictId { get; set; }

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

        /// <summary>
        /// The specific locality within a Geography District, e.g. "Feroke".
        /// Mandatory - it is now the store's only source of coordinates
        /// (see StoreAppService.ResolveLocationAsync); City is overridden
        /// with the resolved district name so the two stay consistent.
        /// </summary>
        [Required]
        public Guid? LocationId { get; set; }

        [Phone]
        [MaxLength(30)]
        public string Phone { get; set; }

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

        [Required]
        public Guid? LocationId { get; set; }

        [Phone]
        [MaxLength(30)]
        public string Phone { get; set; }

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
