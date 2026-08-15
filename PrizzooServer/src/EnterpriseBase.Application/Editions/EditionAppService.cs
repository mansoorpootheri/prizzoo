using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using EnterpriseBase.Application.Editions.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.Editions;
using EnterpriseBase.Editions.Dto;
using EnterpriseBase.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Editions
{
    [AbpAuthorize(PermissionNames.Pages_Administration_Host_Settings)]
    public class EditionAppService : ApplicationService, IEditionAppService
    {
        private readonly EditionManager _editionManager;
        private readonly IRepository<Edition, int> _editionRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;

        public EditionAppService(
            EditionManager editionManager,
            IRepository<Edition, int> editionRepository,
            IRepository<Tenant, int> tenantRepository)
        {
            _editionManager = editionManager;
            _editionRepository = editionRepository;
            _tenantRepository = tenantRepository;
        }

        public async Task<ListResultDto<EditionListDto>> GetEditions()
        {
            var editions = await _editionRepository.GetAll()
                .OfType<EnterpriseEdition>()
                .OrderBy(e => e.DisplayName)
                .ToListAsync();

            var tenantCounts = await _tenantRepository.GetAll()
                .Where(t => t.EditionId.HasValue)
                .GroupBy(t => t.EditionId.Value)
                .Select(g => new { EditionId = g.Key, Count = g.Count() })
                .ToListAsync();

            var dtos = ObjectMapper.Map<List<EditionListDto>>(editions);
            foreach (var dto in dtos)
            {
                dto.TenantCount = tenantCounts.FirstOrDefault(x => x.EditionId == dto.Id)?.Count ?? 0;
                var edition = editions.First(e => e.Id == dto.Id);
                dto.MonthlyPriceExclGst = edition.MonthlyPriceExclGst;
                dto.MonthlyGstAmount = edition.MonthlyGstAmount;
                dto.AnnualPriceExclGst = edition.AnnualPriceExclGst;
                dto.AnnualGstAmount = edition.AnnualGstAmount;
                dto.IsFree = edition.IsFree;
            }

            return new ListResultDto<EditionListDto>(dtos);
        }

        public async Task<GetEditionEditOutput> GetEditionForEdit(NullableIdDto input)
        {
            var features = FeatureManager.GetAll();

            EditionEditDto editionEditDto;
            List<Abp.NameValue> featureValues;

            if (input.Id.HasValue)
            {
                var edition = await _editionRepository.GetAsync(input.Id.Value) as EnterpriseEdition
                    ?? throw new UserFriendlyException("Edition not found.");
                featureValues = (await _editionManager.GetFeatureValuesAsync(input.Id.Value)).ToList();
                editionEditDto = ObjectMapper.Map<EditionEditDto>(edition);
            }
            else
            {
                editionEditDto = new EditionEditDto();
                featureValues = features.Select(f => new Abp.NameValue(f.Name, f.DefaultValue)).ToList();
            }

            var featureDtos = ObjectMapper.Map<List<EnterpriseBase.Editions.Dto.FlatFeatureDto>>(features).OrderBy(f => f.DisplayName).ToList();

            return new GetEditionEditOutput
            {
                Edition = editionEditDto,
                Features = featureDtos,
                FeatureValues = featureValues.Select(fv => new NameValueDto(fv)).ToList()
            };
        }

        public async Task CreateOrUpdateEdition(CreateOrUpdateEditionDto input)
        {
            if (!input.Edition.Id.HasValue)
            {
                await CreateEditionAsync(input);
            }
            else
            {
                await UpdateEditionAsync(input);
            }
        }

        public async Task DeleteEdition(EntityDto input)
        {
            var edition = await _editionRepository.GetAsync(input.Id);
            await _editionManager.DeleteAsync(edition);
        }

        public async Task<List<SubscribableEditionComboboxItemDto>> GetEditionComboboxItems(int? selectedEditionId = null, bool addAllItem = false, bool onlyFreeItems = false)
        {
            var editions = await _editionRepository.GetAll()
                .OrderBy(e => e.DisplayName)
                .ToListAsync();

            var editionItems = new ListResultDto<SubscribableEditionComboboxItemDto>(
                editions.Select(e => new SubscribableEditionComboboxItemDto(e.Id.ToString(), e.DisplayName, e.Id == selectedEditionId)).ToList()
            ).Items.ToList();

            if (addAllItem)
            {
                editionItems.Insert(0, new SubscribableEditionComboboxItemDto("", L("AllEditions"), false));
            }

            return editionItems;
        }

        private async Task CreateEditionAsync(CreateOrUpdateEditionDto input)
        {
            var edition = ObjectMapper.Map<EnterpriseEdition>(input.Edition);
            edition.Name = edition.Name.Trim();

            if (await _editionRepository.FirstOrDefaultAsync(e => e.Name == edition.Name) != null)
                throw new UserFriendlyException(string.Format(L("EditionAlreadyExists"), edition.Name));

            await _editionManager.CreateAsync(edition);
            await CurrentUnitOfWork.SaveChangesAsync();
            await SetFeatureValues(edition, input.FeatureValues);
        }

        private async Task UpdateEditionAsync(CreateOrUpdateEditionDto input)
        {
            var edition = await _editionRepository.GetAsync(input.Edition.Id.Value) as EnterpriseEdition
                ?? throw new UserFriendlyException("Edition not found.");

            edition.Name = input.Edition.Name.Trim();
            edition.DisplayName = input.Edition.DisplayName.Trim();
            edition.MonthlyPrice = input.Edition.MonthlyPrice;
            edition.AnnualPrice = input.Edition.AnnualPrice;
            edition.GstRate = input.Edition.GstRate;
            edition.IsPriceInclusiveOfGst = input.Edition.IsPriceInclusiveOfGst;
            edition.HsnSacCode = input.Edition.HsnSacCode;
            edition.TrialDayCount = input.Edition.TrialDayCount;
            edition.ExpiringEditionId = input.Edition.ExpiringEditionId;

            await SetFeatureValues(edition, input.FeatureValues);
        }

        private async Task SetFeatureValues(Edition edition, List<NameValueDto> featureValues)
        {
            foreach (var featureValue in featureValues)
            {
                await _editionManager.SetFeatureValueAsync(edition.Id, featureValue.Name, featureValue.Value);
            }
        }
    }
}