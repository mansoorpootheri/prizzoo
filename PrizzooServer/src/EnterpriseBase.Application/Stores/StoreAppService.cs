using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using EnterpriseBase.Application.Stores.Dto;
using EnterpriseBase.Authorization;
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

    // Admin/internal-ops CRUD for stores - approve, edit, deactivate.
    // This is separate from the public search that ranks stores by distance;
    // see EnterpriseBase.Application.Pricing.PriceCompareAppService for that.
    [AbpAuthorize(PermissionNames.Pages_Stores)]
    public class StoreAppService : EnterpriseBaseAppServiceBase, IStoreAppService
    {
        private readonly IRepository<Store, Guid> _repository;

        public StoreAppService(IRepository<Store, Guid> repository)
        {
            _repository = repository;
        }

        [UnitOfWork]
        public virtual async Task<StoreDto> GetAsync(EntityDto<Guid> input)
        {
            var store = await _repository.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (store == null)
                throw new UserFriendlyException("Store not found");

            return MapToDto(store);
        }

        [UnitOfWork]
        public virtual async Task<PagedResultDto<StoreDto>> GetAllAsync(PagedStoreRequestDto input)
        {
            var query = _repository.GetAll()
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
            var store = new Store
            {
                Name          = input.Name,
                Address       = input.Address,
                City          = input.City,
                Phone         = input.Phone,
                Latitude      = input.Latitude,
                Longitude     = input.Longitude,
                OpeningHours  = input.OpeningHours,
                CategoryTags  = input.CategoryTags,
                ImageId       = input.ImageId,
                IsActive      = input.IsActive,
                IsVerified    = false, // verification is a separate moderation step
            };

            await _repository.InsertAsync(store);
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapToDto(store);
        }

        [UnitOfWork]
        public virtual async Task<StoreDto> UpdateAsync(UpdateStoreDto input)
        {
            var store = await _repository.GetAsync(input.Id);

            store.Name          = input.Name;
            store.Address       = input.Address;
            store.City          = input.City;
            store.Phone         = input.Phone;
            store.Latitude      = input.Latitude;
            store.Longitude     = input.Longitude;
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

        private static StoreDto MapToDto(Store x)
        {
            return new StoreDto
            {
                Id           = x.Id,
                Name         = x.Name,
                Address      = x.Address,
                City         = x.City,
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
