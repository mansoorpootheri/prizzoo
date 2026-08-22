using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EnterpriseBase.Application.Pricing.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.MasterData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Pricing
{
    public interface IPriceCompareAppService : IApplicationService
    {
        Task<List<StorePriceResultDto>> ComparePricesAsync(ComparePricesInputDto input);

        /// <summary>Active category names for the home page's browse chips - real catalog data, not a hardcoded list.</summary>
        Task<List<string>> GetActiveCategoriesAsync();

        /// <summary>
        /// The home screen's default, no-search-needed product listing:
        /// a "Top Picks for You" row plus one row per catalog category,
        /// each populated from real nearby approved prices.
        /// </summary>
        Task<List<HomeFeedSectionDto>> GetHomeFeedAsync(HomeFeedInputDto input);
    }

    /// <summary>
    /// THE core shopper-facing service: search a product, see it ranked by
    /// price across nearby stores.
    ///
    /// DELIBERATE REVERSAL: this used to be [AllowAnonymous] (no login
    /// required). The product now requires phone+OTP verification before any
    /// browsing, so this is gated to the OTP-verified Shopper role instead -
    /// see OtpAuthController for how that role is granted. This is a
    /// breaking change for any existing anonymous caller.
    ///
    /// PERFORMANCE NOTE: this MVP implementation does a rough SQL bounding-box
    /// pre-filter then computes exact Haversine distance in C#. That is fine
    /// for one city's worth of stores. Before multi-city scale, push the
    /// distance calculation into Postgres itself via the `earthdistance` +
    /// `cube` extensions (or PostGIS once further along) so the database does
    /// the filtering, not the app server. See the India-market business plan,
    /// section 5/9, for when that upgrade is expected to matter.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Shopper)]
    public class PriceCompareAppService : ApplicationService, IPriceCompareAppService
    {
        // Prices older than this are still shown, but flagged IsStale so the
        // UI can visually de-emphasise them - freshness is the product's core
        // trust signal, per the business plan.
        private static readonly TimeSpan FreshnessThreshold = TimeSpan.FromDays(7);

        private readonly IRepository<EnterpriseBase.Pricing.Price, Guid> _priceRepository;
        private readonly IRepository<EnterpriseBase.Pricing.FlyerProduct, Guid> _flyerProductRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<EnterpriseBase.Pricing.ProductRating, Guid> _ratingRepository;

        public PriceCompareAppService(
            IRepository<EnterpriseBase.Pricing.Price, Guid> priceRepository,
            IRepository<EnterpriseBase.Pricing.FlyerProduct, Guid> flyerProductRepository,
            IRepository<Category, Guid> categoryRepository,
            IRepository<EnterpriseBase.Pricing.ProductRating, Guid> ratingRepository)
        {
            _priceRepository = priceRepository;
            _flyerProductRepository = flyerProductRepository;
            _categoryRepository = categoryRepository;
            _ratingRepository = ratingRepository;
        }

        public virtual async Task<List<string>> GetActiveCategoriesAsync()
        {
            return await _categoryRepository.GetAll()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .ToListAsync();
        }

        public virtual async Task<List<StorePriceResultDto>> ComparePricesAsync(ComparePricesInputDto input)
        {
            // A flyer click needs to reliably show that flyer's own items
            // regardless of distance - GetRecentFlyersAsync (home screen's
            // all-stores carousel) isn't geo-scoped, so a flyer from outside
            // the shopper's radius could be on screen; silently returning
            // zero results for a valid tap would be confusing. Bypass the
            // bounding-box/radius filtering entirely in that case.
            List<EnterpriseBase.Pricing.Price> candidates;
            if (input.FlyerId.HasValue)
            {
                // FlyerProduct carries no price of its own - a flyer tap
                // just narrows the normal Price query down to that flyer's
                // store and the set of products it features (see
                // FlyerAppService's doc comment), so the item's price is
                // always whatever that product is currently priced at
                // there.
                var flyerLinks = await _flyerProductRepository.GetAll()
                    .Where(x => x.FlyerId == input.FlyerId.Value)
                    .Select(x => new { x.ProductId, x.Flyer.StoreId })
                    .ToListAsync();
                var flyerStoreId = flyerLinks.Select(x => x.StoreId).FirstOrDefault();
                var flyerProductIds = flyerLinks.Select(x => x.ProductId).ToList();

                candidates = await _priceRepository.GetAll()
                    .Include(x => x.Product).ThenInclude(p => p.Category)
                    .Include(x => x.Store)
                    .Where(x => x.Status == EnterpriseBase.Pricing.PriceStatus.Approved)
                    .Where(x => x.Store.IsActive)
                    .Where(x => x.Product.IsActive)
                    .Where(x => x.StoreId == flyerStoreId && flyerProductIds.Contains(x.ProductId))
                    .ToListAsync();
            }
            else
            {
                candidates = await GetCandidatesInBoundingBoxAsync(input.Latitude, input.Longitude, input.RadiusKm,
                    query =>
                    {
                        if (!string.IsNullOrWhiteSpace(input.ProductKeyword))
                        {
                            var keyword = input.ProductKeyword.ToLower();
                            query = query.Where(x => x.Product.Name.ToLower().Contains(keyword));
                        }

                        if (!string.IsNullOrWhiteSpace(input.CategoryName))
                        {
                            var categoryName = input.CategoryName.ToLower();
                            query = query.Where(x => x.Product.Category != null && x.Product.Category.Name.ToLower() == categoryName);
                        }

                        if (input.StoreId.HasValue)
                        {
                            query = query.Where(x => x.StoreId == input.StoreId.Value);
                        }

                        return query;
                    });
            }

            // A product can have more than one approved Price row at the same
            // store - Price is append-only (see the class doc comment on
            // Price.cs), so re-entering a price (e.g. via a flyer upload
            // after an earlier direct entry) adds a new row rather than
            // overwriting the old one. Only the most recent observation per
            // product+store should ever reach a shopper - older rows stay in
            // the table for history/audit, but showing both here would look
            // like the same item listed twice at the same store.
            candidates = candidates
                .GroupBy(x => new { x.ProductId, x.StoreId })
                .Select(g => g.OrderByDescending(x => x.ObservedAt).First())
                .ToList();

            var results = await BuildResultDtosAsync(candidates, input.Latitude, input.Longitude);

            var filtered = input.FlyerId.HasValue
                ? results.AsEnumerable()
                : results.Where(x => x.DistanceKm <= input.RadiusKm);

            return filtered
                .OrderBy(x => x.Amount)
                .ThenBy(x => x.DistanceKm)
                .Take(input.MaxResults)
                .ToList();
        }

        public virtual async Task<List<HomeFeedSectionDto>> GetHomeFeedAsync(HomeFeedInputDto input)
        {
            const int itemsPerSection = 10;

            var candidates = await GetCandidatesInBoundingBoxAsync(input.Latitude, input.Longitude, input.RadiusKm);
            var allResults = (await BuildResultDtosAsync(candidates, input.Latitude, input.Longitude))
                .Where(x => x.DistanceKm <= input.RadiusKm)
                .ToList();

            // One card per product on the home feed (unlike search results,
            // which intentionally show every store) - the cheapest nearby
            // store wins the card; tapping it still leads to the full
            // per-store comparison via ComparePricesAsync.
            var cheapestPerProduct = allResults
                .GroupBy(x => x.ProductId)
                .Select(g => g.OrderBy(x => x.Amount).First())
                .ToList();

            var categoryNameByProduct = candidates
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => g.First().Product.Category?.Name);

            var sections = new List<HomeFeedSectionDto>();

            var topPicks = cheapestPerProduct
                .OrderByDescending(x => x.AverageRating ?? 0)
                .ThenByDescending(x => x.ObservedAt)
                .Take(itemsPerSection)
                .ToList();
            if (topPicks.Count > 0)
            {
                sections.Add(new HomeFeedSectionDto
                {
                    Title = "Top Picks for You",
                    Items = topPicks,
                });
            }

            var categoryGroups = cheapestPerProduct
                .Select(x => new { Item = x, CategoryName = categoryNameByProduct.GetValueOrDefault(x.ProductId) })
                .Where(x => !string.IsNullOrEmpty(x.CategoryName))
                .GroupBy(x => x.CategoryName)
                .OrderBy(g => g.Key);

            foreach (var group in categoryGroups)
            {
                sections.Add(new HomeFeedSectionDto
                {
                    Title = group.Key,
                    Items = group
                        .Select(x => x.Item)
                        .OrderByDescending(x => x.ObservedAt)
                        .Take(itemsPerSection)
                        .ToList(),
                });
            }

            return sections;
        }

        private async Task<List<EnterpriseBase.Pricing.Price>> GetCandidatesInBoundingBoxAsync(
            decimal latitude,
            decimal longitude,
            double radiusKm,
            Func<IQueryable<EnterpriseBase.Pricing.Price>, IQueryable<EnterpriseBase.Pricing.Price>> extraFilter = null)
        {
            // Rough bounding box in degrees - ~1 degree of latitude is ~111km.
            // Deliberately generous; exact filtering happens after this in C#.
            var latDelta = (decimal)(radiusKm / 111.0);
            var lonDelta = (decimal)(radiusKm / (111.0 * Math.Cos(ToRadians((double)latitude))));

            var minLat = latitude - latDelta;
            var maxLat = latitude + latDelta;
            var minLon = longitude - lonDelta;
            var maxLon = longitude + lonDelta;

            var query = _priceRepository.GetAll()
                .Include(x => x.Product).ThenInclude(p => p.Category)
                .Include(x => x.Store)
                .Where(x => x.Status == EnterpriseBase.Pricing.PriceStatus.Approved)
                .Where(x => x.Store.IsActive)
                .Where(x => x.Product.IsActive)
                .Where(x => x.Store.Latitude >= minLat && x.Store.Latitude <= maxLat)
                .Where(x => x.Store.Longitude >= minLon && x.Store.Longitude <= maxLon);

            if (extraFilter != null)
                query = extraFilter(query);

            return await query.ToListAsync();
        }

        private async Task<List<StorePriceResultDto>> BuildResultDtosAsync(
            List<EnterpriseBase.Pricing.Price> candidates, decimal latitude, decimal longitude)
        {
            var now = DateTime.UtcNow;

            var productIds = candidates.Select(x => x.ProductId).Distinct().ToList();
            var ratingsByProduct = (await _ratingRepository.GetAll()
                    .Where(x => productIds.Contains(x.ProductId))
                    .Select(x => new { x.ProductId, x.Stars })
                    .ToListAsync())
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => (Average: g.Average(x => x.Stars), Count: g.Count()));

            return candidates
                .Select(x =>
                {
                    ratingsByProduct.TryGetValue(x.ProductId, out var rating);
                    var discountPercent = x.OriginalAmount.HasValue && x.OriginalAmount.Value > x.Amount
                        ? (int?)Math.Round((1 - (x.Amount / x.OriginalAmount.Value)) * 100)
                        : null;

                    return new StorePriceResultDto
                    {
                        PriceId         = x.Id,
                        ProductId       = x.ProductId,
                        ProductName     = x.Product.Name,
                        ProductImageId  = x.Product.ImageId,
                        StoreId         = x.StoreId,
                        StoreName       = x.Store.Name,
                        StoreImageId    = x.Store.ImageId,
                        StoreAddress    = x.Store.Address,
                        Latitude        = x.Store.Latitude,
                        Longitude       = x.Store.Longitude,
                        DistanceKm      = HaversineKm(latitude, longitude, x.Store.Latitude, x.Store.Longitude),
                        Amount          = x.Amount,
                        OriginalAmount  = x.OriginalAmount,
                        DiscountPercent = discountPercent,
                        Currency        = x.Currency,
                        ObservedAt      = x.ObservedAt,
                        IsStale         = now - x.ObservedAt > FreshnessThreshold,
                        AverageRating   = rating.Count > 0 ? rating.Average : (double?)null,
                        RatingCount     = rating.Count,
                    };
                })
                .ToList();
        }

        private static double HaversineKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double earthRadiusKm = 6371;
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
