using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.MasterData.Units.Dto
{
    public class UnitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int DecimalPlaces { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEditUnitDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; }

        [Range(0, 6)]
        public int DecimalPlaces { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
