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

        /// <summary>
        /// Populated once, only in the CreateAsync response, so the host
        /// admin can relay it to the shop owner manually (no WhatsApp/SMS
        /// credential delivery yet). Never returned by any other endpoint.
        /// </summary>
        public string OwnerTemporaryPassword { get; set; }
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
        /// Optional - if set, StoreAppService overrides City with the
        /// resolved district name so the two stay consistent.
        /// </summary>
        public Guid? LocationId { get; set; }

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

        // Owner account - host admin creates the shop AND its owner login in
        // one action, there is no self-service retailer signup. See
        // StoreAppService.CreateAsync.
        [Required]
        [MaxLength(64)]
        public string OwnerName { get; set; }

        [MaxLength(64)]
        public string OwnerSurname { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string OwnerEmail { get; set; }

        [Phone]
        [MaxLength(30)]
        public string OwnerPhoneNumber { get; set; }
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

        public Guid? LocationId { get; set; }

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
