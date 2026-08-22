using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using EnterpriseBase.Application.Stores.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.MasterData;
using EnterpriseBase.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Stores
{
    public interface IStoreAppService : IApplicationService
    {
        Task<StoreDto> GetAsync(EntityDto<Guid> input);
        Task<PagedResultDto<StoreDto>> GetAllAsync(PagedStoreRequestDto input);
        Task<StoreDto> CreateAsync(CreateStoreDto input);
        Task<StoreDto> UpdateAsync(UpdateStoreDto input);
        Task DeleteAsync(EntityDto<Guid> input);
    }

    // Admin CRUD for stores - approve, edit, deactivate. This is separate
    // from the public search that ranks stores by distance; see
    // EnterpriseBase.Application.Pricing.PriceCompareAppService for that.
    // A single Admin identity (phone+OTP) manages every store directly -
    // there is no per-store owner login to provision here.
    [AbpAuthorize(PermissionNames.Pages_Stores)]
    public class StoreAppService : EnterpriseBaseAppServiceBase, IStoreAppService
    {
        private readonly IRepository<Store, Guid> _repository;
        private readonly IRepository<Location, Guid> _locationRepository;

        public StoreAppService(IRepository<Store, Guid> repository, IRepository<Location, Guid> locationRepository)
        {
            _repository = repository;
            _locationRepository = locationRepository;
        }

        [UnitOfWork]
        public virtual async Task<StoreDto> GetAsync(EntityDto<Guid> input)
        {
            var store = await _repository.GetAll()
                .Include(x => x.Location)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            if (store == null)
                throw new UserFriendlyException("Store not found");

            return MapToDto(store);
        }

        [UnitOfWork]
        public virtual async Task<PagedResultDto<StoreDto>> GetAllAsync(PagedStoreRequestDto input)
        {
            var query = _repository.GetAll()
                .Include(x => x.Location)
                .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Name.ToLower().Contains(input.Keyword.ToLower()))
                .WhereIf(!string.IsNullOrEmpty(input.City), x => x.City == input.City)
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive.Value)
                .WhereIf(input.IsVerified.HasValue, x => x.IsVerified == input.IsVerified.Value);

            var totalCount = await query.CountAsync();

            var stores = await query
                .OrderBy(x => x.Name)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<StoreDto>(totalCount, stores.Select(MapToDto).ToList());
        }

        [UnitOfWork]
        public virtual async Task<StoreDto> CreateAsync(CreateStoreDto input)
        {
            var location = await ResolveLocationAsync(input.LocationId!.Value);

            var store = new Store
            {
                Name          = input.Name,
                Address       = input.Address,
                // Location is the sole source of both City and coordinates now.
                City          = location.District.DistrictName,
                LocationId    = location.Id,
                Location      = location,
                Phone         = input.Phone,
                Latitude      = location.Latitude!.Value,
                Longitude     = location.Longitude!.Value,
                OpeningHours  = input.OpeningHours,
                CategoryTags  = input.CategoryTags,
                ImageId       = input.ImageId,
                IsActive      = input.IsActive,
                IsVerified    = true, // admin creating it IS the verification - no separate approval step
            };

            await _repository.InsertAsync(store);
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapToDto(store);
        }

        [UnitOfWork]
        public virtual async Task<StoreDto> UpdateAsync(UpdateStoreDto input)
        {
            var store = await _repository.GetAsync(input.Id);
            var location = await ResolveLocationAsync(input.LocationId!.Value);

            store.Name          = input.Name;
            store.Address       = input.Address;
            store.City          = location.District.DistrictName;
            store.LocationId    = location.Id;
            store.Location      = location;
            store.Phone         = input.Phone;
            store.Latitude      = location.Latitude!.Value;
            store.Longitude     = location.Longitude!.Value;
            store.OpeningHours  = input.OpeningHours;
            store.CategoryTags  = input.CategoryTags;
            store.ImageId       = input.ImageId;
            store.IsVerified    = input.IsVerified;
            store.IsActive      = input.IsActive;

            await _repository.UpdateAsync(store);
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapToDto(store);
        }

        [UnitOfWork]
        public virtual async Task DeleteAsync(EntityDto<Guid> input)
        {
            await _repository.DeleteAsync(input.Id);
        }

        // LocationId is [Required] on both input DTOs, so locationId here is
        // always a real value by the time this runs - a store's coordinates
        // come from its Location, never from client-supplied lat/lng.
        private async Task<Location> ResolveLocationAsync(Guid locationId)
        {
            var location = await _locationRepository.GetAll()
                .Include(x => x.District)
                .FirstOrDefaultAsync(x => x.Id == locationId);
            if (location == null)
                throw new UserFriendlyException("Selected location not found.");
            if (location.Latitude == null || location.Longitude == null)
                throw new UserFriendlyException(
                    "This location doesn't have coordinates set yet. Ask an admin to set this location's coordinates (Admin > Locations) before assigning it to a store.");

            return location;
        }

        private static StoreDto MapToDto(Store x)
        {
            return new StoreDto
            {
                Id           = x.Id,
                Name         = x.Name,
                Address      = x.Address,
                City         = x.City,
                LocationId   = x.LocationId,
                LocationName = x.Location?.Name,
                DistrictId   = x.Location?.DistrictId,
                Phone        = x.Phone,
                Latitude     = x.Latitude,
                Longitude    = x.Longitude,
                OpeningHours = x.OpeningHours,
                CategoryTags = x.CategoryTags,
                IsVerified   = x.IsVerified,
                IsActive     = x.IsActive,
                ImageId      = x.ImageId,
            };
        }
    }
}
