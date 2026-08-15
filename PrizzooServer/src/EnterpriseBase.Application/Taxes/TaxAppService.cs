using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EnterpriseBase.Authorization;
using EnterpriseBase.Taxes.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Taxes
{
    [AbpAuthorize(PermissionNames.Pages_Administration_Taxes)]
    public class TaxAppService : ApplicationService, ITaxAppService
    {
        private readonly IRepository<Tax, Guid> _repository;

        public TaxAppService(IRepository<Tax, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<List<TaxDto>> GetAllAsync()
        {
            var list = await _repository.GetAll().OrderBy(x => x.Name).ToListAsync();
            return list.Select(x => new TaxDto
            {
                Id                   = x.Id,
                Name                 = x.Name,
                TaxCode              = x.TaxCode,
                TaxType              = x.TaxType,
                Percentage           = x.Percentage,
                ApplyOnServiceCharge = x.ApplyOnServiceCharge,
                IsActive             = x.IsActive
            }).ToList();
        }

        public async Task CreateOrEditAsync(CreateEditTaxDto input)
        {
            if (!input.Id.HasValue || input.Id == Guid.Empty)
            {
                await _repository.InsertAsync(new Tax
                {
                    Id                   = Guid.NewGuid(),
                    Name                 = input.Name,
                    TaxCode              = input.TaxCode,
                    TaxType              = input.TaxType,
                    Percentage           = input.Percentage,
                    ApplyOnServiceCharge = input.ApplyOnServiceCharge,
                    IsActive             = input.IsActive
                });
            }
            else
            {
                var entity = await _repository.GetAsync(input.Id.Value);
                entity.Name                 = input.Name;
                entity.TaxCode              = input.TaxCode;
                entity.TaxType              = input.TaxType;
                entity.Percentage           = input.Percentage;
                entity.ApplyOnServiceCharge = input.ApplyOnServiceCharge;
                entity.IsActive             = input.IsActive;
            }
        }

        [AbpAuthorize(PermissionNames.Pages_Administration_Taxes_Delete)]
        public async Task DeleteAsync(EntityDto<Guid> input)
        {
            await _repository.DeleteAsync(input.Id);
        }

        public async Task<List<ComboboxItemDto>> GetForComboboxAsync()
        {
            var list = await _repository.GetAll().OrderBy(x => x.Name).ToListAsync();
            return list.Select(x => new ComboboxItemDto
            {
                Value       = x.Id.ToString(),
                DisplayText = $"{x.Name} ({x.Percentage}%)"
            }).ToList();
        }
    }
}
