using Abp.Application.Services.Dto;
using System;

namespace EnterpriseBase.Employees.Dto
{
    public class CreateEmployeeEditDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public int? EmployeeTypeId { get; set; }
        public bool Status { get; set; } = true;
        public int? BranchId { get; set; }
        public string EmployeeNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string AlternatePhone { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string EmergencyContactRelation { get; set; }
        public decimal? BasicSalary { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string PANNumber { get; set; }
        public string AadharNumber { get; set; }
        public string Notes { get; set; }
        public long? UserId { get; set; }
    }
}