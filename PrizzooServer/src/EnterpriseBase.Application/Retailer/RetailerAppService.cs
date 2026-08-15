using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using EnterpriseBase.Application.MasterData.Products.Dto;
using EnterpriseBase.Application.Retailer.Dto;
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

namespace EnterpriseBase.Application.Retailer
{
    public interface IRetailerAppService : Abp.Application.Services.IApplicationService
    {
        /// <summary>Any authenticated user: apply to become a retailer.</summary>
        Task ApplyAsync(CreateStoreDto input);

        /// <summary>Any authenticated user: check their own application status.</summary>
        Task<RetailerApplicationStatusDto> GetMyApplicationStatusAsync();

        /// <summary>Moderator: list applications awaiting review.</summary>
        Task<List<RetailerApplicationListDto>> GetPendingApplicationsAsync();

        /// <summary>Moderator: approve an application - verifies the store and grants the Retailer role.</summary>
        Task ApproveApplicationAsync(EntityDto<Guid> input);

        /// <summary>Moderator: reject an application (soft-deletes the store so the applicant can re-apply).</summary>
        Task RejectApplicationAsync(RejectApplicationDto input);

        /// <summary>Retailer: view their own verified store.</summary>
        Task<StoreDto> GetMyStoreAsync();

        /// <summary>Retailer: update their own verified store.</summary>
        Task UpdateMyStoreAsync(UpdateMyStoreDto input);

        Task<List<ComboboxItemDto>> GetCategoriesForDropdownAsync();
        Task<List<ComboboxItemDto>> GetUnitsForDropdownAsync();

        /// <summary>Retailer: propose a new catalog product (goes live only after moderation).</summary>
        Task<ProductDto> CreateMyProductAsync(CreateProductDto input);

        /// <summary>Retailer: list products they've submitted.</summary>
        Task<List<ProductDto>> GetMyProductsAsync();
    }

    /// <summary>
    /// Retailer self-service: apply -&gt; (moderated) -&gt; manage own store/products.
    /// Price submission is NOT here - the retailer calls the existing
    /// PriceSubmissionAppService.SubmitAsync directly with their own StoreId;
    /// that endpoint already works for any authenticated user, unchanged.
    /// </summary>
    public class RetailerAppService : EnterpriseBaseAppServiceBase, IRetailerAppService
    {
        private readonly IRepository<Store, Guid> _storeRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<Unit, Guid> _unitRepository;

        public RetailerAppService(
            IRepository<Store, Guid> storeRepository,
            IRepository<Product, Guid> productRepository,
            IRepository<Category, Guid> categoryRepository,
            IRepository<Unit, Guid> unitRepository)
        {
            _storeRepository = storeRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitRepository = unitRepository;
        }

        [AbpAuthorize]
        [UnitOfWork]
        public virtual async Task ApplyAsync(CreateStoreDto input)
        {
            var existing = await _storeRepository.FirstOrDefaultAsync(x => x.OwnerUserId == AbpSession.UserId);
            if (existing != null)
                throw new UserFriendlyException("You already have a store application on file.");

            var store = new Store
            {
                Name = input.Name,
                Address = input.Address,
                City = input.City,
                Phone = input.Phone,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                OpeningHours = input.OpeningHours,
                CategoryTags = input.CategoryTags,
                ImageId = input.ImageId,
                OwnerUserId = AbpSession.UserId,
                IsVerified = false,
                IsActive = true,
            };

            await _storeRepository.InsertAsync(store);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize]
        [UnitOfWork]
        public virtual async Task<RetailerApplicationStatusDto> GetMyApplicationStatusAsync()
        {
            var store = await _storeRepository.FirstOrDefaultAsync(x => x.OwnerUserId == AbpSession.UserId);
            return new RetailerApplicationStatusDto
            {
                HasApplied = store != null,
                IsApproved = store?.IsVerified ?? false,
                StoreId = store?.Id,
                StoreName = store?.Name
            };
        }

        [AbpAuthorize(PermissionNames.Pages_RetailerModeration)]
        [UnitOfWork]
        public virtual async Task<List<RetailerApplicationListDto>> GetPendingApplicationsAsync()
        {
            var pending = await _storeRepository.GetAll()
                .Where(x => x.OwnerUserId.HasValue && !x.IsVerified)
                .OrderBy(x => x.CreationTime)
                .ToListAsync();

            var ownerIds = pending.Select(x => x.OwnerUserId.Value).Distinct().ToList();
            var owners = await UserManager.Users
                .Where(u => ownerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            return pending.Select(x => new RetailerApplicationListDto
            {
                Id = x.Id,
                StoreName = x.Name,
                City = x.City,
                OwnerUserId = x.OwnerUserId,
                OwnerName = owners.TryGetValue(x.OwnerUserId.Value, out var owner) ? $"{owner.Name} {owner.Surname}" : null,
                OwnerEmail = owners.TryGetValue(x.OwnerUserId.Value, out var owner2) ? owner2.EmailAddress : null,
                CreationTime = x.CreationTime
            }).ToList();
        }

        [AbpAuthorize(PermissionNames.Pages_RetailerModeration)]
        [UnitOfWork]
        public virtual async Task ApproveApplicationAsync(EntityDto<Guid> input)
        {
            var store = await _storeRepository.GetAsync(input.Id);
            if (!store.OwnerUserId.HasValue)
                throw new UserFriendlyException("This store has no applicant to approve.");

            store.IsVerified = true;
            await _storeRepository.UpdateAsync(store);

            var owner = await UserManager.FindByIdAsync(store.OwnerUserId.Value.ToString());
            if (owner == null)
                throw new UserFriendlyException("Applicant user no longer exists.");

            CheckErrors(await UserManager.AddToRoleAsync(owner, StaticRoleNames.Tenants.Retailer));

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(PermissionNames.Pages_RetailerModeration)]
        [UnitOfWork]
        public virtual async Task RejectApplicationAsync(RejectApplicationDto input)
        {
            var store = await _storeRepository.GetAsync(input.Id);
            if (store.IsVerified)
                throw new UserFriendlyException("Cannot reject an already-approved store.");

            // Soft-delete (Store is FullAuditedEntity/ISoftDelete): the row
            // disappears from all normal queries, so GetMyApplicationStatusAsync
            // reports "not applied" again and the user can simply re-apply.
            await _storeRepository.DeleteAsync(store.Id);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(PermissionNames.Pages_Retailer)]
        [UnitOfWork]
        public virtual async Task<StoreDto> GetMyStoreAsync()
        {
            var store = await _storeRepository.FirstOrDefaultAsync(
                x => x.OwnerUserId == AbpSession.UserId && x.IsVerified);
            if (store == null)
                throw new UserFriendlyException("No verified store found for this account.");

            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Address = store.Address,
                City = store.City,
                Phone = store.Phone,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                OpeningHours = store.OpeningHours,
                CategoryTags = store.CategoryTags,
                IsVerified = store.IsVerified,
                IsActive = store.IsActive,
                ImageId = store.ImageId
            };
        }

        [AbpAuthorize(PermissionNames.Pages_Retailer)]
        [UnitOfWork]
        public virtual async Task UpdateMyStoreAsync(UpdateMyStoreDto input)
        {
            var store = await _storeRepository.FirstOrDefaultAsync(
                x => x.OwnerUserId == AbpSession.UserId && x.IsVerified);
            if (store == null)
                throw new UserFriendlyException("No verified store found for this account.");

            store.Name = input.Name;
            store.Address = input.Address;
            store.City = input.City;
            store.Phone = input.Phone;
            store.Latitude = input.Latitude;
            store.Longitude = input.Longitude;
            store.OpeningHours = input.OpeningHours;
            store.CategoryTags = input.CategoryTags;
            store.ImageId = input.ImageId;

            await _storeRepository.UpdateAsync(store);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(PermissionNames.Pages_Retailer)]
        public virtual async Task<List<ComboboxItemDto>> GetCategoriesForDropdownAsync()
        {
            var list = await _categoryRepository.GetAll().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
            return list.Select(x => new ComboboxItemDto { Value = x.Id.ToString(), DisplayText = x.Name }).ToList();
        }

        [AbpAuthorize(PermissionNames.Pages_Retailer)]
        public virtual async Task<List<ComboboxItemDto>> GetUnitsForDropdownAsync()
        {
            var list = await _unitRepository.GetAll().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
            return list.Select(x => new ComboboxItemDto { Value = x.Id.ToString(), DisplayText = x.Name }).ToList();
        }

        [AbpAuthorize(PermissionNames.Pages_Retailer)]
        [UnitOfWork]
        public virtual async Task<ProductDto> CreateMyProductAsync(CreateProductDto input)
        {
            var product = new Product
            {
                Name = input.Name,
                Barcode = input.Barcode,
                Description = input.Description,
                CategoryId = input.CategoryId,
                UnitId = input.UnitId,
                ImageId = input.ImageId,
                IsActive = false // always pending moderation, regardless of what the caller sent
            };

            await _productRepository.InsertAsync(product);
            await CurrentUnitOfWork.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                Description = product.Description,
                CategoryId = product.CategoryId,
                UnitId = product.UnitId,
                ImageId = product.ImageId,
                IsActive = product.IsActive
            };
        }

        [AbpAuthorize(PermissionNames.Pages_Retailer)]
        [UnitOfWork]
        public virtual async Task<List<ProductDto>> GetMyProductsAsync()
        {
            var mine = await _productRepository.GetAll()
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .Where(x => x.CreatorUserId == AbpSession.UserId)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();

            return mine.Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Barcode = x.Barcode,
                Description = x.Description,
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : null,
                UnitId = x.UnitId,
                UnitName = x.Unit != null ? x.Unit.Name : null,
                ImageId = x.ImageId,
                IsActive = x.IsActive
            }).ToList();
        }
    }
}
