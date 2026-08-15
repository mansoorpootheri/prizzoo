using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EnterpriseBase.Application.MasterData.Units.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.MasterData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unit = EnterpriseBase.MasterData.Unit;

namespace EnterpriseBase.Application.MasterData.Units
{
    [AbpAuthorize(PermissionNames.Pages_Products_Units)]
    public class UnitAppService : EnterpriseBaseAppServiceBase, IUnitAppService
    {
        private readonly IRepository<Unit, Guid> _repository;

        public UnitAppService(IRepository<Unit, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<List<UnitDto>> GetAllAsync()
        {
            var list = await _repository.GetAll().OrderBy(x => x.Name).ToListAsync();
            return list.Select(x => new UnitDto
            {
                Id            = x.Id,
                Name          = x.Name,
                Code          = x.Code,
                DecimalPlaces = x.DecimalPlaces,
                IsActive      = x.IsActive
            }).ToList();
        }

        [AbpAuthorize(PermissionNames.Pages_Products_Units_Create, PermissionNames.Pages_Products_Units_Edit)]
        public async Task CreateOrEditAsync(CreateEditUnitDto input)
        {
            if (!input.Id.HasValue || input.Id == Guid.Empty)
            {
                await _repository.InsertAsync(new Unit
                {
                    Id            = Guid.NewGuid(),
                    Name          = input.Name,
                    Code          = input.Code,
                    DecimalPlaces = input.DecimalPlaces,
                    IsActive      = input.IsActive
                });
            }
            else
            {
                var entity = await _repository.GetAsync(input.Id.Value);
                entity.Name          = input.Name;
                entity.Code          = input.Code;
                entity.DecimalPlaces = input.DecimalPlaces;
                entity.IsActive      = input.IsActive;
            }
        }

        [AbpAuthorize(PermissionNames.Pages_Products_Units_Delete)]
        public async Task DeleteAsync(EntityDto<Guid> input)
        {
            await _repository.DeleteAsync(input.Id);
        }

        public async Task<List<ComboboxItemDto>> GetForComboboxAsync()
        {
            var list = await _repository.GetAll()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return list.Select(x => new ComboboxItemDto
            {
                Value       = x.Id.ToString(),
                DisplayText = x.Name
            }).ToList();
        }
    }
}
