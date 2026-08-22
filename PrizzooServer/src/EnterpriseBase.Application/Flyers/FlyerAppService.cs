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

        /// <summary>Admin: add more items to a flyer that's already live - same "pre-verified actor, goes live immediately" pattern as the initial upload.</summary>
        Task AddItemsToFlyerAsync(AddFlyerItemsDto input);

        /// <summary>Shopper: every flyer uploaded for a store, newest first, each with the items typed in alongside it - empty list if the store has none. The frontend browses these as a carousel.</summary>
        Task<List<FlyerDetailDto>> GetFlyersForStoreAsync(EntityDto<Guid> input);

        /// <summary>Shopper: the most recently uploaded flyers across every store (not scoped to one), newest first - backs the home screen's "In the spotlight"-style carousel shown before any store is selected.</summary>
        Task<List<FlyerDetailDto>> GetRecentFlyersAsync();

        /// <summary>Admin: remove a flyer entirely (soft delete, same as every other FullAuditedEntity in this codebase) - its FlyerProduct links go with it (they're filtered out along with their soft-deleted parent), but any Price rows it caused to be created stay on record, since those are now independent, legitimate prices.</summary>
        Task DeleteAsync(EntityDto<Guid> input);
    }

    /// <summary>
    /// Deliberately minimal (phase 1 - see the plan this shipped from):
    /// no OCR/AI extraction, no shop-owner self-upload, no separate
    /// moderation queue. An admin types the flyer's items directly; each
    /// becomes a FlyerProduct row linking this flyer to the Product it
    /// features - a master/detail link, not a Price row. A Price row is
    /// only ever inserted for an item the first time that product has no
    /// existing price at this store; otherwise the item's price is simply
    /// whatever that product is already priced at here (see
    /// InsertItemsAsync/BuildFlyerDetailDtosAsync), so a flyer never
    /// creates a duplicate/second Price for a product already on record.
    /// A future OCR pass can pre-fill the item list this service accepts
    /// without changing anything here.
    /// </summary>
    public class FlyerAppService : ApplicationService, IFlyerAppService
    {
        private readonly IRepository<Flyer, Guid> _flyerRepository;
        private readonly IRepository<FlyerProduct, Guid> _flyerProductRepository;
        private readonly IRepository<Store, Guid> _storeRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Price, Guid> _priceRepository;

        public FlyerAppService(
            IRepository<Flyer, Guid> flyerRepository,
            IRepository<FlyerProduct, Guid> flyerProductRepository,
            IRepository<Store, Guid> storeRepository,
            IRepository<Product, Guid> productRepository,
            IRepository<Price, Guid> priceRepository)
        {
            _flyerRepository = flyerRepository;
            _flyerProductRepository = flyerProductRepository;
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

            await InsertItemsAsync(flyer, input.Items);
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

        [AbpAuthorize(PermissionNames.Pages_PriceModeration)]
        [UnitOfWork]
        public virtual async Task AddItemsToFlyerAsync(AddFlyerItemsDto input)
        {
            var flyer = await _flyerRepository.GetAll().FirstOrDefaultAsync(x => x.Id == input.FlyerId);
            if (flyer == null)
                throw new UserFriendlyException("Flyer not found.");

            await InsertItemsAsync(flyer, input.Items);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task InsertItemsAsync(Flyer flyer, List<FlyerLineItemDto> items)
        {
            foreach (var item in items)
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

                var alreadyLinked = await _flyerProductRepository.GetAll()
                    .AnyAsync(x => x.FlyerId == flyer.Id && x.ProductId == productId);
                if (alreadyLinked)
                    continue; // adding the same product to the same flyer twice is a no-op, not an error

                var hasPriceAtStore = await _priceRepository.GetAll()
                    .AnyAsync(p => p.ProductId == productId && p.StoreId == flyer.StoreId && p.Status == PriceStatus.Approved);

                if (!hasPriceAtStore)
                {
                    // First time this product has ever been priced at this
                    // store - a genuinely new price, not a duplicate.
                    await _priceRepository.InsertAsync(new Price
                    {
                        ProductId = productId,
                        StoreId = flyer.StoreId,
                        Amount = item.Price,
                        OriginalAmount = item.OriginalAmount,
                        Currency = "INR",
                        Source = PriceSource.RetailerReported,
                        Status = PriceStatus.Approved,
                        SubmittedByUserId = AbpSession.UserId,
                        ObservedAt = DateTime.UtcNow,
                    });
                }

                await _flyerProductRepository.InsertAsync(new FlyerProduct
                {
                    FlyerId = flyer.Id,
                    ProductId = productId,
                });
            }
        }

        [AbpAuthorize(PermissionNames.Pages_PriceModeration)]
        [UnitOfWork]
        public virtual async Task DeleteAsync(EntityDto<Guid> input)
        {
            await _flyerRepository.DeleteAsync(input.Id);
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
            var storeIdByFlyerId = flyers.ToDictionary(x => x.Id, x => x.StoreId);

            var links = await _flyerProductRepository.GetAll()
                .Include(x => x.Product)
                .Where(x => flyerIds.Contains(x.FlyerId))
                .ToListAsync();

            // Batch-fetch every (StoreId, ProductId) pair's latest approved
            // price in one query, then match in memory - same "fetch broad,
            // group in memory" style already used for ratings lookups
            // elsewhere in this codebase, avoids one query per link.
            var storeIds = links.Select(x => storeIdByFlyerId[x.FlyerId]).Distinct().ToList();
            var productIds = links.Select(x => x.ProductId).Distinct().ToList();
            var latestPriceByStoreProduct = (await _priceRepository.GetAll()
                    .Where(p => p.Status == PriceStatus.Approved && storeIds.Contains(p.StoreId) && productIds.Contains(p.ProductId))
                    .ToListAsync())
                .GroupBy(p => (p.StoreId, p.ProductId))
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.ObservedAt).First());

            var itemsByFlyerId = new Dictionary<Guid, List<FlyerLineItemResultDto>>();
            foreach (var link in links)
            {
                var storeId = storeIdByFlyerId[link.FlyerId];
                if (!latestPriceByStoreProduct.TryGetValue((storeId, link.ProductId), out var price))
                    continue; // no price on record for this product at this store - shouldn't happen given the insert-time invariant, skip defensively

                if (!itemsByFlyerId.TryGetValue(link.FlyerId, out var list))
                {
                    list = new List<FlyerLineItemResultDto>();
                    itemsByFlyerId[link.FlyerId] = list;
                }
                list.Add(new FlyerLineItemResultDto
                {
                    ProductName = link.Product.Name,
                    Amount = price.Amount,
                    OriginalAmount = price.OriginalAmount,
                });
            }

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
        /// attaching this price to the wrong item. A new product goes live
        /// immediately since the admin creating it is already trusted, no
        /// separate moderation step.
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
