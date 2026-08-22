using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.Flyers.Dto
{
    public class FlyerLineItemDto
    {
        /// <summary>Pick an existing catalog product directly - when set, Name/CategoryId are ignored. Exactly one of ProductId or Name must be supplied (validated in FlyerAppService, not via attributes, since the requirement is conditional).</summary>
        public Guid? ProductId { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>Only used when creating a new product (ProductId is null) - which category it belongs to. Optional even then; a product can be uncategorized.</summary>
        public Guid? CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        /// <summary>Optional MRP/original price, for computing a discount badge. Must exceed Price to have any effect.</summary>
        [Range(0.01, double.MaxValue)]
        public decimal? OriginalAmount { get; set; }
    }

    public class CreateFlyerForStoreDto
    {
        [Required]
        public Guid StoreId { get; set; }

        [Required]
        public Guid ImageId { get; set; }

        [Required]
        [MinLength(1)]
        public List<FlyerLineItemDto> Items { get; set; }
    }

    public class FlyerDto : EntityDto<Guid>
    {
        public Guid StoreId { get; set; }
        public Guid ImageId { get; set; }
        public DateTime UploadedAt { get; set; }
        public int ItemCount { get; set; }
    }

    public class FlyerLineItemResultDto
    {
        public string ProductName { get; set; }
        public decimal Amount { get; set; }
        public decimal? OriginalAmount { get; set; }
    }

    /// <summary>Full view of a store's live flyer - the photo plus every item that was typed in alongside it, each a real (Approved) Price row. See FlyerAppService.GetLiveFlyerForStoreAsync.</summary>
    public class FlyerDetailDto : EntityDto<Guid>
    {
        public Guid StoreId { get; set; }
        public string StoreName { get; set; }
        public Guid ImageId { get; set; }
        public List<FlyerLineItemResultDto> Items { get; set; } = new List<FlyerLineItemResultDto>();
    }
}
