using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using EnterpriseBase.Application.Flyers.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.MasterData;
using EnterpriseBase.Pricing;
using EnterpriseBase.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Flyers
{
    public interface IFlyerAppService : IApplicationService
    {
        /// <summary>Admin: upload a flyer photo for a store together with its item list, typed in by hand. Goes live immediately - there is no moderation step, since the admin's own submission is the trusted source (see CreateMyProductAsync for the same "pre-verified actor, no pending step" precedent elsewhere in this codebase).</summary>
        Task<FlyerDto> CreateFlyerForStoreAsync(CreateFlyerForStoreDto input);

        /// <summary>Shopper: every flyer uploaded for a store, newest first, each with the items typed in alongside it - empty list if the store has none. The frontend browses these as a carousel.</summary>
        Task<List<FlyerDetailDto>> GetFlyersForStoreAsync(EntityDto<Guid> input);

        /// <summary>Shopper: the most recently uploaded flyers across every store (not scoped to one), newest first - backs the home screen's "In the spotlight"-style carousel shown before any store is selected.</summary>
        Task<List<FlyerDetailDto>> GetRecentFlyersAsync();
    }

    /// <summary>
    /// Deliberately minimal (phase 1 - see the plan this shipped from):
    /// no OCR/AI extraction, no shop-owner self-upload, no separate
    /// moderation queue. An admin types the flyer's items directly; each
    /// becomes a real Price row (Source = PriceSource.Flyer, Status =
    /// Approved, FlyerId = this flyer) so it flows through
    /// PriceCompareAppService's existing search/home-feed/freshness
    /// machinery unchanged - there is no separate "flyer price" display
    /// path. A future OCR pass can pre-fill the item list this service
    /// accepts without changing anything here.
    /// </summary>
    public class FlyerAppService : ApplicationService, IFlyerAppService
    {
        private readonly IRepository<Flyer, Guid> _flyerRepository;
        private readonly IRepository<Store, Guid> _storeRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Price, Guid> _priceRepository;

        public FlyerAppService(
            IRepository<Flyer, Guid> flyerRepository,
            IRepository<Store, Guid> storeRepository,
            IRepository<Product, Guid> productRepository,
            IRepository<Price, Guid> priceRepository)
        {
            _flyerRepository = flyerRepository;
            _storeRepository = storeRepository;
            _productRepository = productRepository;
            _priceRepository = priceRepository;
        }

        [AbpAuthorize(PermissionNames.Pages_PriceModeration)]
        [UnitOfWork]
        public virtual async Task<FlyerDto> CreateFlyerForStoreAsync(CreateFlyerForStoreDto input)
        {
            var storeExists = await _storeRepository.GetAll().AnyAsync(x => x.Id == input.StoreId);
            if (!storeExists)
                throw new UserFriendlyException("Store not found.");

            if (input.Items == null || input.Items.Count == 0)
                throw new UserFriendlyException("Add at least one item.");

            var flyer = new Flyer
            {
                StoreId = input.StoreId,
                ImageId = input.ImageId,
                UploadedByUserId = AbpSession.UserId,
                UploadedAt = DateTime.UtcNow,
            };
            await _flyerRepository.InsertAsync(flyer);
            await CurrentUnitOfWork.SaveChangesAsync();

            foreach (var item in input.Items)
            {
                Guid productId;
                if (item.ProductId.HasValue)
                {
                    var exists = await _productRepository.GetAll().AnyAsync(x => x.Id == item.ProductId.Value);
                    if (!exists)
                        throw new UserFriendlyException("Selected product not found.");
                    productId = item.ProductId.Value;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(item.Name))
                        throw new UserFriendlyException("Each item needs a product - pick an existing one or type a name for a new one.");
                    productId = await FindOrCreateProductAsync(item.Name, item.CategoryId);
                }

                await _priceRepository.InsertAsync(new Price
                {
                    ProductId = productId,
                    StoreId = input.StoreId,
                    Amount = item.Price,
                    OriginalAmount = item.OriginalAmount,
                    Currency = "INR",
                    Source = PriceSource.Flyer,
                    Status = PriceStatus.Approved,
                    SubmittedByUserId = AbpSession.UserId,
                    ObservedAt = flyer.UploadedAt,
                    FlyerId = flyer.Id,
                });
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            return new FlyerDto
            {
                Id = flyer.Id,
                StoreId = flyer.StoreId,
                ImageId = flyer.ImageId,
                UploadedAt = flyer.UploadedAt,
                ItemCount = input.Items.Count,
            };
        }

        [AbpAuthorize(PermissionNames.Pages_Shopper)]
        [UnitOfWork]
        public virtual async Task<List<FlyerDetailDto>> GetFlyersForStoreAsync(EntityDto<Guid> input)
        {
            var flyers = await _flyerRepository.GetAll()
                .Include(x => x.Store)
                .Where(x => x.StoreId == input.Id)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();

            return await BuildFlyerDetailDtosAsync(flyers);
        }

        [AbpAuthorize(PermissionNames.Pages_Shopper)]
        [UnitOfWork]
        public virtual async Task<List<FlyerDetailDto>> GetRecentFlyersAsync()
        {
            var flyers = await _flyerRepository.GetAll()
                .Include(x => x.Store)
                .OrderByDescending(x => x.UploadedAt)
                .Take(RecentFlyersMaxCount)
                .ToListAsync();

            return await BuildFlyerDetailDtosAsync(flyers);
        }

        private const int RecentFlyersMaxCount = 20;

        private async Task<List<FlyerDetailDto>> BuildFlyerDetailDtosAsync(List<Flyer> flyers)
        {
            if (flyers.Count == 0)
                return new List<FlyerDetailDto>();

            var flyerIds = flyers.Select(x => x.Id).ToList();
            var allItems = await _priceRepository.GetAll()
                .Include(x => x.Product)
                .Where(x => x.FlyerId.HasValue && flyerIds.Contains(x.FlyerId.Value) && x.Status == PriceStatus.Approved)
                .Select(x => new { x.FlyerId, Item = new FlyerLineItemResultDto
                {
                    ProductName = x.Product.Name,
                    Amount = x.Amount,
                    OriginalAmount = x.OriginalAmount,
                } })
                .ToListAsync();
            var itemsByFlyerId = allItems
                .GroupBy(x => x.FlyerId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Item).ToList());

            return flyers.Select(flyer => new FlyerDetailDto
            {
                Id = flyer.Id,
                StoreId = flyer.StoreId,
                StoreName = flyer.Store?.Name,
                ImageId = flyer.ImageId,
                Items = itemsByFlyerId.TryGetValue(flyer.Id, out var items) ? items : new List<FlyerLineItemResultDto>(),
            }).ToList();
        }

        /// <summary>
        /// Exact (case-insensitive) match against an existing active
        /// Product's name; creates a new one if none exists. Exact, not
        /// fuzzy - the admin is typing the real product name directly, so
        /// guessing a "close enough" existing product risks silently
        /// attaching this price to the wrong item. Mirrors
        /// ShopOwnerAppService.CreateMyProductAsync: a pre-verified actor's
        /// new product goes live immediately, no separate moderation.
        /// </summary>
        private async Task<Guid> FindOrCreateProductAsync(string name, Guid? categoryId)
        {
            var trimmedName = name.Trim();
            var needle = trimmedName.ToLower();

            var existingId = await _productRepository.GetAll()
                .Where(x => x.IsActive && x.Name.ToLower() == needle)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
            if (existingId != Guid.Empty)
                return existingId;

            var product = new Product { Name = trimmedName, CategoryId = categoryId, IsActive = true };
            await _productRepository.InsertAsync(product);
            await CurrentUnitOfWork.SaveChangesAsync();
            return product.Id;
        }
    }
}
