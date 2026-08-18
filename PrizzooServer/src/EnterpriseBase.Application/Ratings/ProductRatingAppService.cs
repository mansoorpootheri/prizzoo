using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using EnterpriseBase.Application.Ratings.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.Pricing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Ratings
{
    public interface IProductRatingAppService : IApplicationService
    {
        /// <summary>Shopper: rate a product 1-5 stars. Rating again overwrites the previous value.</summary>
        Task RateProductAsync(RateProductDto input);

        /// <summary>Shopper: the caller's own existing rating for a product, if any (to preselect the star widget).</summary>
        Task<MyProductRatingDto> GetMyRatingAsync(Guid productId);
    }

    /// <summary>
    /// Backs the star rating shown on product listing cards. Deliberately
    /// product-scoped, not store-scoped - the same product's rating is the
    /// same regardless of which store's price a shopper is looking at.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Shopper)]
    public class ProductRatingAppService : ApplicationService, IProductRatingAppService
    {
        private readonly IRepository<ProductRating, Guid> _ratingRepository;

        public ProductRatingAppService(IRepository<ProductRating, Guid> ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        [UnitOfWork]
        public virtual async Task RateProductAsync(RateProductDto input)
        {
            var userId = AbpSession.GetUserId();

            var existing = await _ratingRepository.GetAll()
                .FirstOrDefaultAsync(x => x.ProductId == input.ProductId && x.ShopperUserId == userId);

            if (existing != null)
            {
                existing.Stars = input.Stars;
                await _ratingRepository.UpdateAsync(existing);
            }
            else
            {
                await _ratingRepository.InsertAsync(new ProductRating
                {
                    ProductId = input.ProductId,
                    ShopperUserId = userId,
                    Stars = input.Stars,
                });
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [UnitOfWork]
        public virtual async Task<MyProductRatingDto> GetMyRatingAsync(Guid productId)
        {
            var userId = AbpSession.GetUserId();

            var existing = await _ratingRepository.GetAll()
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.ShopperUserId == userId);

            return new MyProductRatingDto
            {
                ProductId = productId,
                Stars = existing?.Stars,
            };
        }
    }
}
