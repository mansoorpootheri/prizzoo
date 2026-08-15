using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EnterpriseBase.Application.Pricing.Dto;
using Microsoft.AspNetCore.Authorization;
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
    }

    /// <summary>
    /// THE core shopper-facing service: search a product, see it ranked by
    /// price across nearby stores. Deliberately [AllowAnonymous] - a shopper
    /// should never need to log in just to compare prices. See
    /// DOCS/PRIZZOO_MIGRATION_NOTES.md for the reasoning behind keeping this
    /// as its own anonymous service rather than folding it into ProductAppService.
    ///
    /// PERFORMANCE NOTE: this MVP implementation does a rough SQL bounding-box
    /// pre-filter then computes exact Haversine distance in C#. That is fine
    /// for one city's worth of stores. Before multi-city scale, push the
    /// distance calculation into Postgres itself via the `earthdistance` +
    /// `cube` extensions (or PostGIS once further along) so the database does
    /// the filtering, not the app server. See the India-market business plan,
    /// section 5/9, for when that upgrade is expected to matter.
    /// </summary>
    [AllowAnonymous]
    public class PriceCompareAppService : ApplicationService, IPriceCompareAppService
    {
        // Prices older than this are still shown, but flagged IsStale so the
        // UI can visually de-emphasise them - freshness is the product's core
        // trust signal, per the business plan.
        private static readonly TimeSpan FreshnessThreshold = TimeSpan.FromDays(7);

        private readonly IRepository<EnterpriseBase.Pricing.Price, Guid> _priceRepository;

        public PriceCompareAppService(IRepository<EnterpriseBase.Pricing.Price, Guid> priceRepository)
        {
            _priceRepository = priceRepository;
        }

        public virtual async Task<List<StorePriceResultDto>> ComparePricesAsync(ComparePricesInputDto input)
        {
            // Rough bounding box in degrees - ~1 degree of latitude is ~111km.
            // Deliberately generous; exact filtering happens below in C#.
            var latDelta = (decimal)(input.RadiusKm / 111.0);
            var lonDelta = (decimal)(input.RadiusKm / (111.0 * Math.Cos(ToRadians((double)input.Latitude))));

            var minLat = input.Latitude - latDelta;
            var maxLat = input.Latitude + latDelta;
            var minLon = input.Longitude - lonDelta;
            var maxLon = input.Longitude + lonDelta;

            var keyword = input.ProductKeyword.ToLower();

            var candidates = await _priceRepository.GetAll()
                .Include(x => x.Product)
                .Include(x => x.Store)
                .Where(x => x.Status == EnterpriseBase.Pricing.PriceStatus.Approved)
                .Where(x => x.Store.IsActive)
                .Where(x => x.Product.IsActive)
                .Where(x => x.Product.Name.ToLower().Contains(keyword))
                .Where(x => x.Store.Latitude >= minLat && x.Store.Latitude <= maxLat)
                .Where(x => x.Store.Longitude >= minLon && x.Store.Longitude <= maxLon)
                .ToListAsync();

            var now = DateTime.UtcNow;

            var results = candidates
                .Select(x => new StorePriceResultDto
                {
                    PriceId      = x.Id,
                    ProductId    = x.ProductId,
                    ProductName  = x.Product.Name,
                    StoreId      = x.StoreId,
                    StoreName    = x.Store.Name,
                    StoreAddress = x.Store.Address,
                    Latitude     = x.Store.Latitude,
                    Longitude    = x.Store.Longitude,
                    DistanceKm   = HaversineKm(input.Latitude, input.Longitude, x.Store.Latitude, x.Store.Longitude),
                    Amount       = x.Amount,
                    Currency     = x.Currency,
                    ObservedAt   = x.ObservedAt,
                    IsStale      = now - x.ObservedAt > FreshnessThreshold,
                })
                .Where(x => x.DistanceKm <= input.RadiusKm)
                .OrderBy(x => x.Amount)
                .ThenBy(x => x.DistanceKm)
                .Take(input.MaxResults)
                .ToList();

            return results;
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
