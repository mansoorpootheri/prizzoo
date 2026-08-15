using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Runtime.Validation;
using Abp.Timing;
using EnterpriseBase.Configuration.Dto;
using EnterpriseBase.Configuration.Host.Dto;

namespace EnterpriseBase.Configuration.Tenants.Dto
{
    public class TenantSettingsEditDto
    {
        public GeneralSettingsEditDto General { get; set; }

        [Required]
        public TenantUserManagementSettingsEditDto UserManagement { get; set; }

        [Required]
        public UiSettingsEditDto Ui { get; set; }

        public CompanySettingsEditDto Company { get; set; }

        public FinanceSettingsEditDto Finance { get; set; }

        public PrintSettingsEditDto Print { get; set; }


        public TenantSettingsEditDto()
        {
        }

        /// <summary>
        /// This validation is done for single-tenant applications.
        /// Because, these settings can only be set by tenant in a single-tenant application.
        /// </summary>
        public void ValidateHostSettings()
        {
            var validationErrors = new List<ValidationResult>();
            if (Clock.SupportsMultipleTimezone && General == null)
            {
                validationErrors.Add(new ValidationResult("General settings can not be null", new[] { "General" }));
            }

            if (validationErrors.Count > 0)
            {
                throw new AbpValidationException("Method arguments are not valid! See ValidationErrors for details.", validationErrors);
            }
        }
    }
}