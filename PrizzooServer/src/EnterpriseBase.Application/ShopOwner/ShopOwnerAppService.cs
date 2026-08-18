using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using EnterpriseBase.Application.MasterData.Products.Dto;
using EnterpriseBase.Application.ShopOwner.Dto;
using EnterpriseBase.Application.Stores.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.MasterData;
using EnterpriseBase.Pricing;
using EnterpriseBase.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.ShopOwner
{
    public interface IShopOwnerAppService : IApplicationService
    {
        /// <summary>Shop owner: view their own store's details.</summary>
        Task<StoreDto> GetMyStoreAsync();

        /// <summary>Shop owner: view their own products with each one's latest submitted price.</summary>
        Task<List<MyStoreProductDto>> GetMyProductsAndPricesAsync();

        /// <summary>Shop owner: propose a new catalog product.</summary>
        Task<ProductDto> CreateMyProductAsync(CreateProductDto input);

        /// <summary>
        /// Shop owner: submit a new price observation (append-only, so
        /// "updating a price" means adding a new row, never editing one in
        /// place) for their own store. Always lands Pending moderation.
        /// </summary>
        Task SubmitPriceForMyStoreAsync(SubmitMyPriceDto input);

        Task<List<ComboboxItemDto>> GetCategoriesForDropdownAsync();
        Task<List<ComboboxItemDto>> GetUnitsForDropdownAsync();
    }

    /// <summary>
    /// Minimal shop-owner self-service portal: view own store + products/
    /// prices, add a product, submit a price. Distinct from
    /// PriceSubmissionAppService.SubmitAsync (which stays as-is, open to any
    /// authenticated user, Source=Crowdsourced) - SubmitPriceForMyStoreAsync
    /// here resolves the caller's own store server-side and sets
    /// Source=RetailerReported, so a shop owner can never submit a price for
    /// a store they don't own.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_ShopOwner)]
    public class ShopOwnerAppService : EnterpriseBaseAppServiceBase, IShopOwnerAppService
    {
        private readonly IRepository<Store, Guid> _storeRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<Unit, Guid> _unitRepository;
        private readonly IRepository<Price, Guid> _priceRepository;

        public ShopOwnerAppService(
            IRepository<Store, Guid> storeRepository,
            IRepository<Product, Guid> productRepository,
            IRepository<Category, Guid> categoryRepository,
            IRepository<Unit, Guid> unitRepository,
            IRepository<Price, Guid> priceRepository)
        {
            _storeRepository = storeRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitRepository = unitRepository;
            _priceRepository = priceRepository;
        }

        private async Task<Store> GetMyStoreEntityAsync()
        {
            var store = await _storeRepository.GetAll()
                .Include(x => x.Location)
                .FirstOrDefaultAsync(x => x.OwnerUserId == AbpSession.UserId);
            if (store == null)
                throw new UserFriendlyException("No store is linked to this account.");

            return store;
        }

        [UnitOfWork]
        public virtual async Task<StoreDto> GetMyStoreAsync()
        {
            var store = await GetMyStoreEntityAsync();

            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Address = store.Address,
                City = store.City,
                LocationId = store.LocationId,
                LocationName = store.Location?.Name,
                DistrictId = store.Location?.DistrictId,
                Phone = store.Phone,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                OpeningHours = store.OpeningHours,
                CategoryTags = store.CategoryTags,
                IsVerified = store.IsVerified,
                IsActive = store.IsActive,
                ImageId = store.ImageId,
            };
        }

        [UnitOfWork]
        public virtual async Task<List<MyStoreProductDto>> GetMyProductsAndPricesAsync()
        {
            var store = await GetMyStoreEntityAsync();

            var storePrices = await _priceRepository.GetAll()
                .Where(x => x.StoreId == store.Id)
                .ToListAsync();

            var latestPrices = storePrices
                .GroupBy(x => x.ProductId)
                .Select(g => g.OrderByDescending(p => p.ObservedAt).First())
                .ToList();

            var productIds = latestPrices.Select(x => x.ProductId).ToList();
            var products = await _productRepository.GetAll()
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .Where(x => productIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            return latestPrices.Select(p =>
            {
                products.TryGetValue(p.ProductId, out var product);
                return new MyStoreProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = product?.Name,
                    CategoryName = product?.Category?.Name,
                    UnitName = product?.Unit?.Name,
                    ImageId = product?.ImageId,
                    LatestPriceAmount = p.Amount,
                    LatestPriceStatus = p.Status,
                    LatestPriceObservedAt = p.ObservedAt,
                };
            }).ToList();
        }

        [UnitOfWork]
        public virtual async Task<ProductDto> CreateMyProductAsync(CreateProductDto input)
        {
            // Not force-pending: unlike the old self-apply flow, host-created
            // shop owners are pre-verified, so their products go live
            // immediately. Only the price itself is moderated (see
            // SubmitPriceForMyStoreAsync / PriceSubmissionAppService).
            var product = new Product
            {
                Name = input.Name,
                Barcode = input.Barcode,
                Description = input.Description,
                CategoryId = input.CategoryId,
                UnitId = input.UnitId,
                ImageId = input.ImageId,
                IsActive = true,
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
                IsActive = product.IsActive,
            };
        }

        [UnitOfWork]
        public virtual async Task SubmitPriceForMyStoreAsync(SubmitMyPriceDto input)
        {
            var store = await GetMyStoreEntityAsync();

            var productExists = await _productRepository.GetAll().AnyAsync(x => x.Id == input.ProductId);
            if (!productExists)
                throw new UserFriendlyException("Product not found.");

            var price = new Price
            {
                ProductId = input.ProductId,
                StoreId = store.Id,
                Amount = input.Amount,
                OriginalAmount = input.OriginalAmount,
                Currency = "INR",
                Source = PriceSource.RetailerReported,
                Status = PriceStatus.Pending,
                SubmittedByUserId = AbpSession.UserId,
                ProofImageId = input.ProofImageId,
                ObservedAt = DateTime.UtcNow,
            };

            await _priceRepository.InsertAsync(price);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public virtual async Task<List<ComboboxItemDto>> GetCategoriesForDropdownAsync()
        {
            var list = await _categoryRepository.GetAll().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
            return list.Select(x => new ComboboxItemDto { Value = x.Id.ToString(), DisplayText = x.Name }).ToList();
        }

        public virtual async Task<List<ComboboxItemDto>> GetUnitsForDropdownAsync()
        {
            var list = await _unitRepository.GetAll().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
            return list.Select(x => new ComboboxItemDto { Value = x.Id.ToString(), DisplayText = x.Name }).ToList();
        }
    }
}
