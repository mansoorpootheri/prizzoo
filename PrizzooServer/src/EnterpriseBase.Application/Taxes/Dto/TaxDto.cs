using EnterpriseBase.Taxes;
using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Taxes.Dto
{
    public class TaxDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TaxCode { get; set; }
        public TaxType TaxType { get; set; }
        public decimal Percentage { get; set; }
        public bool ApplyOnServiceCharge { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEditTaxDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string TaxCode { get; set; }

        public TaxType TaxType { get; set; }

        [Range(0, 100)]
        public decimal Percentage { get; set; }

        public bool ApplyOnServiceCharge { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }
}
