using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.DynamicEntityProperties;
using Abp.EntityHistory;
using Abp.Extensions;
using Abp.Localization;
using Abp.Notifications;
using Abp.Organizations;
using Abp.UI.Inputs;
using Abp.Webhooks;
using AutoMapper;
using EnterpriseBase.Authorization.Accounts.Dto;
using EnterpriseBase.Authorization.Delegation;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Editions;
using EnterpriseBase.Editions.Dto;
using EnterpriseBase.MultiTenancy;
using EnterpriseBase.MultiTenancy.Dto;
using EnterpriseBase.Sessions.Dto;
using EnterpriseBase.Geography;
using EnterpriseBase.Geography.Dto;
using Newtonsoft.Json;
using System.Collections.Generic;
using EnterpriseBase.Application.Subscriptions.Dto;

namespace EnterpriseBase
{
    internal static class CustomDtoMapper
    {
        public static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            //Inputs
            configuration.CreateMap<CheckboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<SingleLineStringInputType, FeatureInputTypeDto>();
            configuration.CreateMap<ComboboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<IInputType, FeatureInputTypeDto>()
                .Include<CheckboxInputType, FeatureInputTypeDto>()
                .Include<SingleLineStringInputType, FeatureInputTypeDto>()
                .Include<ComboboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
            configuration.CreateMap<ILocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>()
                .Include<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
            configuration.CreateMap<LocalizableComboboxItem, LocalizableComboboxItemDto>();
            configuration.CreateMap<ILocalizableComboboxItem, LocalizableComboboxItemDto>()
                .Include<LocalizableComboboxItem, LocalizableComboboxItemDto>();


            //Feature
            configuration.CreateMap<FlatFeatureSelectDto, Feature>().ReverseMap();
            configuration.CreateMap<Feature, FlatFeatureDto>();

            //Geography
            configuration.CreateMap<Country, CountryDto>().ReverseMap();
            configuration.CreateMap<CreateCountryDto, Country>();
            configuration.CreateMap<UpdateCountryDto, Country>();
            configuration.CreateMap<UpdateCountryDto, CountryDto>().ReverseMap();

            configuration.CreateMap<State, StateDto>()
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.CountryName));
            configuration.CreateMap<CreateStateDto, State>();
            configuration.CreateMap<UpdateStateDto, State>();
            configuration.CreateMap<UpdateStateDto, StateDto>().ReverseMap();

            configuration.CreateMap<District, DistrictDto>()
                .ForMember(dest => dest.StateName, opt => opt.MapFrom(src => src.State.StateName))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.State.Country.CountryName));
            configuration.CreateMap<CreateDistrictDto, District>();
            configuration.CreateMap<UpdateDistrictDto, District>();
            configuration.CreateMap<UpdateDistrictDto, DistrictDto>().ReverseMap();           

            //Subscription
            configuration.CreateMap<Edition, EditionInfoDto>();

            // Prizzoo catalog mappings - see EnterpriseBase.Application.Stores /
            // .Pricing / .MasterData.Products for the Dto folders. Add
            // CreateMap entries here as those AppServices/Dtos are built out;
            // left empty deliberately since the Dto shapes weren't defined as
            // part of this migration pass - see DOCS/PRIZZOO_MIGRATION_NOTES.md.
        }
    }
}