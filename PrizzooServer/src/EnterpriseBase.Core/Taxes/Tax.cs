using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Taxes
{
    [Table("Taxes")]
    public class Tax : Entity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string TaxCode { get; set; }

        public TaxType TaxType { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; }

        public bool ApplyOnServiceCharge { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }

    public enum TaxType
    {
        GST  = 1,
        CGST = 2,
        SGST = 3,
        IGST = 4
    }
}
