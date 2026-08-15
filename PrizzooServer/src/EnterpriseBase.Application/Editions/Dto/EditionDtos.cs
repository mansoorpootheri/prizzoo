using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Application.Editions;
using Abp.Application.Features;
using EnterpriseBase.Editions;
using EnterpriseBase.Editions.Dto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.Editions.Dto
{
    [AutoMapFrom(typeof(EnterpriseEdition))]
    public class EditionListDto : EntityDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int? TenantCount { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public decimal? AnnualPrice { get; set; }
        public decimal GstRate { get; set; }
        public bool IsPriceInclusiveOfGst { get; set; }
        public string HsnSacCode { get; set; }
        public int? TrialDayCount { get; set; }
        public bool IsFree { get; set; }
        public decimal? MonthlyPriceExclGst { get; set; }
        public decimal? MonthlyGstAmount { get; set; }
        public decimal? AnnualPriceExclGst { get; set; }
        public decimal? AnnualGstAmount { get; set; }
    }

    [AutoMapFrom(typeof(EnterpriseEdition)), AutoMapTo(typeof(EnterpriseEdition))]
    public class EditionEditDto : EntityDto<int?>
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string DisplayName { get; set; }

        public decimal? MonthlyPrice { get; set; }
        public decimal? AnnualPrice { get; set; }
        public decimal GstRate { get; set; } = 18m;
        public bool IsPriceInclusiveOfGst { get; set; } = false;
        public string HsnSacCode { get; set; } = "998314";
        public int? TrialDayCount { get; set; }
        public int? ExpiringEditionId { get; set; }
    }

    public class CreateOrUpdateEditionDto
    {
        [Required]
        public EditionEditDto Edition { get; set; }

        [Required]
        public List<NameValueDto> FeatureValues { get; set; }
    }

    public class GetEditionEditOutput
    {
        public EditionEditDto Edition { get; set; }
        public List<FlatFeatureDto> Features { get; set; }
        public List<NameValueDto> FeatureValues { get; set; }
    }

    public class SubscribableEditionComboboxItemDto
    {
        public string Value { get; set; }
        public string DisplayText { get; set; }
        public bool IsSelected { get; set; }

        public SubscribableEditionComboboxItemDto()
        {
        }

        public SubscribableEditionComboboxItemDto(string value, string displayText, bool isSelected)
        {
            Value = value;
            DisplayText = displayText;
            IsSelected = isSelected;
        }
    }   
}