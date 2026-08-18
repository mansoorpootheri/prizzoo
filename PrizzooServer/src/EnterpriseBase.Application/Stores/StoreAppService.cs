using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.UI;
using EnterpriseBase.Application.Stores.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.MasterData;
using EnterpriseBase.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    //
    // There is no self-service retailer signup: creating a store also
    // creates its owner's login in the same call (CreateAsync below), which
    // is why this whole service is host-only (Pages_Stores is a Host-side
    // permission) - only the host super admin can provision shops.
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
            var defaultTenant = await TenantManager.FindByTenancyNameAsync(AbpTenantBase.DefaultTenantName);
            if (defaultTenant == null)
                throw new UserFriendlyException("Default tenant not found - cannot provision a shop owner login.");

            User owner;
            string ownerTemporaryPassword;

            // User is tenant-scoped even though Store/Product/Price are not
            // (see Store.cs's doc comment) - create the owner login under the
            // app's single operating tenant, mirroring the host->tenant
            // pattern already used by ImpersonationManager/UserManager.
            using (CurrentUnitOfWork.SetTenantId(defaultTenant.Id))
            {
                owner = new User
                {
                    TenantId = defaultTenant.Id,
                    UserName = input.OwnerEmail,
                    Name = input.OwnerName,
                    Surname = string.IsNullOrWhiteSpace(input.OwnerSurname) ? input.OwnerName : input.OwnerSurname,
                    EmailAddress = input.OwnerEmail,
                    PhoneNumber = input.OwnerPhoneNumber,
                    IsEmailConfirmed = true,
                    IsActive = true,
                    Roles = new List<UserRole>(),
                };
                owner.SetNormalizedNames();

                // Host admin sets no password - a random one-time password is
                // generated and returned once in the response for the admin
                // to relay manually (no WhatsApp/SMS credential delivery yet).
                ownerTemporaryPassword = User.CreateRandomPassword();

                await UserManager.InitializeOptionsAsync(defaultTenant.Id);
                CheckErrors(await UserManager.CreateAsync(owner, ownerTemporaryPassword));
                CheckErrors(await UserManager.AddToRoleAsync(owner, StaticRoleNames.Tenants.ShopOwner));
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var location = await ResolveLocationOrNullAsync(input.LocationId);

            var store = new Store
            {
                Name          = input.Name,
                Address       = input.Address,
                // Location, when picked, is the source of truth for City -
                // overrides any free-text City the caller also sent.
                City          = location?.District.DistrictName ?? input.City,
                LocationId    = location?.Id,
                Location      = location,
                Phone         = input.Phone,
                Latitude      = input.Latitude,
                Longitude     = input.Longitude,
                OpeningHours  = input.OpeningHours,
                CategoryTags  = input.CategoryTags,
                ImageId       = input.ImageId,
                IsActive      = input.IsActive,
                OwnerUserId   = owner.Id,
                IsVerified    = true, // host admin creating it IS the verification now - no separate approval step
            };

            await _repository.InsertAsync(store);
            await CurrentUnitOfWork.SaveChangesAsync();

            var dto = MapToDto(store);
            dto.OwnerTemporaryPassword = ownerTemporaryPassword;
            return dto;
        }

        [UnitOfWork]
        public virtual async Task<StoreDto> UpdateAsync(UpdateStoreDto input)
        {
            var store = await _repository.GetAsync(input.Id);
            var location = await ResolveLocationOrNullAsync(input.LocationId);

            store.Name          = input.Name;
            store.Address       = input.Address;
            store.City          = location?.District.DistrictName ?? input.City;
            store.LocationId    = location?.Id;
            store.Location      = location;
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

        private async Task<Location> ResolveLocationOrNullAsync(Guid? locationId)
        {
            if (!locationId.HasValue)
                return null;

            var location = await _locationRepository.GetAll()
                .Include(x => x.District)
                .FirstOrDefaultAsync(x => x.Id == locationId.Value);
            if (location == null)
                throw new UserFriendlyException("Selected location not found.");

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
