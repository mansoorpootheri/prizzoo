using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using EnterpriseBase.Application.MasterData.Locations.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.MasterData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.MasterData.Locations
{
    public interface ILocationAppService : IApplicationService
    {
        Task<List<LocationDto>> GetAllAsync(int? districtId);
        Task CreateOrEditAsync(CreateEditLocationDto input);
        Task DeleteAsync(EntityDto<Guid> input);
        Task<List<ComboboxItemDto>> GetForComboboxAsync(int districtId);
    }

    /// <summary>
    /// A locality within a Geography District, e.g. "Feroke" within
    /// "Kozhikode". District plays the "city" role in the store-creation
    /// form (see DefaultGeographyCreator) - this narrows within it.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Locations)]
    public class LocationAppService : EnterpriseBaseAppServiceBase, ILocationAppService
    {
        private readonly IRepository<Location, Guid> _repository;

        public LocationAppService(IRepository<Location, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<List<LocationDto>> GetAllAsync(int? districtId)
        {
            var list = await _repository.GetAll()
                .Include(x => x.District)
                .WhereIf(districtId.HasValue, x => x.DistrictId == districtId.Value)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return list.Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                DistrictId = x.DistrictId,
                DistrictName = x.District.DistrictName,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                IsActive = x.IsActive
            }).ToList();
        }

        [AbpAuthorize(PermissionNames.Pages_Locations_Create, PermissionNames.Pages_Locations_Edit)]
        public async Task CreateOrEditAsync(CreateEditLocationDto input)
        {
            if (!input.Id.HasValue || input.Id == Guid.Empty)
            {
                await _repository.InsertAsync(new Location
                {
                    Id = Guid.NewGuid(),
                    Name = input.Name,
                    DistrictId = input.DistrictId,
                    Latitude = input.Latitude,
                    Longitude = input.Longitude,
                    IsActive = input.IsActive
                });
            }
            else
            {
                var entity = await _repository.GetAsync(input.Id.Value);
                entity.Name = input.Name;
                entity.DistrictId = input.DistrictId;
                entity.Latitude = input.Latitude;
                entity.Longitude = input.Longitude;
                entity.IsActive = input.IsActive;
            }
        }

        [AbpAuthorize(PermissionNames.Pages_Locations_Delete)]
        public async Task DeleteAsync(EntityDto<Guid> input)
        {
            await _repository.DeleteAsync(input.Id);
        }

        public async Task<List<ComboboxItemDto>> GetForComboboxAsync(int districtId)
        {
            var list = await _repository.GetAll()
                .Where(x => x.IsActive && x.DistrictId == districtId)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return list.Select(x => new ComboboxItemDto
            {
                Value = x.Id.ToString(),
                DisplayText = x.Name
            }).ToList();
        }
    }
}
