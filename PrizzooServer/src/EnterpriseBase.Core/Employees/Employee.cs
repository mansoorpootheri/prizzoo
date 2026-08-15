using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Branches;
using EnterpriseBase.DataFilters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Employees
{
    [Table("Employees")]
    public class Employee : FullAuditedEntity<long>, IMayHaveTenant, IMustHaveBranch
    {
        public int? TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(20)]
        public string MobileNo { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(10)]
        public string Gender { get; set; }

        public int? EmployeeTypeId { get; set; }
        [ForeignKey("EmployeeTypeId")]
        public virtual EmployeeType EmployeeType { get; set; }

        public bool Status { get; set; } = true;

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        [MaxLength(50)]
        public string EmployeeNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime JoiningDate { get; set; }

        [MaxLength(100)]
        public string Designation { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(20)]
        public string PostalCode { get; set; }

        [MaxLength(20)]
        public string AlternatePhone { get; set; }

        [MaxLength(200)]
        public string EmergencyContactName { get; set; }

        [MaxLength(20)]
        public string EmergencyContactPhone { get; set; }

        [MaxLength(50)]
        public string EmergencyContactRelation { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BasicSalary { get; set; }

        [MaxLength(50)]
        public string BankAccountNumber { get; set; }

        [MaxLength(100)]
        public string BankName { get; set; }

        [MaxLength(20)]
        public string PANNumber { get; set; }

        [MaxLength(20)]
        public string AadharNumber { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; }

        public long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}