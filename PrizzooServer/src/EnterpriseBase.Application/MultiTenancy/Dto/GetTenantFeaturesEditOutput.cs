using System.Collections.Generic;
using EnterpriseBase.Editions.Dto;
using Abp.Application.Services.Dto;

namespace EnterpriseBase.MultiTenancy.Dto
{
    public class GetTenantFeaturesEditOutput
    {
        public List<NameValueDto> FeatureValues { get; set; }

        public List<FlatFeatureDto> Features { get; set; }
    }
}