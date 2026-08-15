using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.MasterData.Products.Dto
{
    // Pricing fields (SKU/CostPrice/SellingPrice) were removed - a product has
    // many prices, one per store, living in EnterpriseBase.Pricing.Price.
    // See DOCS/PRIZZOO_MIGRATION_NOTES.md.
    public class ProductDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Barcode { get; set; }
        public string Description { get; set; }
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid? UnitId { get; set; }
        public string UnitName { get; set; }
        public Guid? ImageId { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Barcode { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public Guid? CategoryId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? ImageId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductDto : EntityDto<Guid>
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Barcode { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public Guid? CategoryId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? ImageId { get; set; }

        public bool IsActive { get; set; }
    }

    public class PagedProductRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}
