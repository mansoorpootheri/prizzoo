using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.Retailer.Dto
{
    public class RetailerApplicationStatusDto
    {
        public bool HasApplied { get; set; }
        public bool IsApproved { get; set; }
        public Guid? StoreId { get; set; }
        public string StoreName { get; set; }
    }

    public class RetailerApplicationListDto : EntityDto<Guid>
    {
        public string StoreName { get; set; }
        public string City { get; set; }
        public long? OwnerUserId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class RejectApplicationDto : EntityDto<Guid>
    {
        // Accepted for a future notification email; not persisted anywhere -
        // Store has no rejection-reason column. Rejection soft-deletes the
        // row so the applicant can simply re-apply (see RetailerAppService).
        public string Reason { get; set; }
    }

    // Deliberately excludes IsVerified/IsActive/OwnerUserId - a retailer must
    // never be able to self-verify or reassign ownership through this DTO.
    public class UpdateMyStoreDto
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
    }
}
