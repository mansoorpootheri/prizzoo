using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using EnterpriseBase.Authorization;
using EnterpriseBase.Application.Pricing.Dto;
using EnterpriseBase.Pricing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Pricing
{
    public interface IPriceSubmissionAppService : IApplicationService
    {
        /// <summary>Shopper-authenticated: submit a crowdsourced price for moderation.</summary>
        Task SubmitAsync(SubmitPriceDto input);

        /// <summary>Admin/moderator-authenticated: list prices awaiting approval.</summary>
        Task<List<PendingPriceDto>> GetPendingAsync();

        /// <summary>Admin/moderator-authenticated: approve, flag, or reject a submission.</summary>
        Task ModerateAsync(ModeratePriceDto input);

        /// <summary>Admin/moderator-authenticated: directly attach an approved
        /// price for a product at a store - skips the pending-moderation queue
        /// since the caller is already the trusted admin, mirroring
        /// FlyerAppService's "pre-verified actor, goes live immediately" pattern.</summary>
        Task CreateApprovedAsync(SubmitPriceDto input);

        /// <summary>Admin/moderator-authenticated: every price ever recorded,
        /// any status - "which store sells this at what price" view.</summary>
        Task<PagedResultDto<AdminPriceDto>> GetAllAsync(PagedPriceRequestDto input);

        /// <summary>Admin/moderator-authenticated: correct an existing price's amount/status.</summary>
        Task UpdateAsync(UpdatePriceDto input);

        /// <summary>Admin/moderator-authenticated: remove a price entered by mistake.</summary>
        Task DeleteAsync(EntityDto<Guid> input);
    }

    /// <summary>
    /// Distinct from PriceCompareAppService (anonymous, read-only) - this
    /// service requires a logged-in shopper to submit and a logged-in
    /// admin/moderator to approve. See DOCS/PRIZZOO_MIGRATION_NOTES.md for
    /// the four-layer service split this follows.
    /// </summary>
    public class PriceSubmissionAppService : ApplicationService, IPriceSubmissionAppService
    {
        private readonly IRepository<Price, Guid> _priceRepository;

        public PriceSubmissionAppService(IRepository<Price, Guid> priceRepository)
        {
            _priceRepository = priceRepository;
        }

        [AbpAuthorize]
        [UnitOfWork]
        public virtual async Task SubmitAsync(SubmitPriceDto input)
        {
            var price = new Price
            {
                ProductId        = input.ProductId,
                StoreId          = input.StoreId,
                Amount           = input.Amount,
                OriginalAmount   = input.OriginalAmount,
                Currency         = "INR",
                Source           = PriceSource.Crowdsourced,
                Status           = PriceStatus.Pending,
                SubmittedByUserId = AbpSession.UserId,
                ProofImageId     = input.ProofImageId,
                ObservedAt       = DateTime.UtcNow,
            };

            await _priceRepository.InsertAsync(price);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(PermissionNames.Pages_PriceModeration)]
        [UnitOfWork]
        public virtual async Task<List<PendingPriceDto>> GetPendingAsync()
        {
            var pending = await _priceRepository.GetAll()
                .Include(x => x.Product)
                .Include(x => x.Store)
                .Where(x => x.Status == PriceStatus.Pending)
                .OrderBy(x => x.ObservedAt)
                .Select(x => new PendingPriceDto
                {
                    Id           = x.Id,
                    ProductName  = x.Product.Name,
                    StoreName    = x.Store.Name,
                    Amount       = x.Amount,
                    ProofImageId = x.ProofImageId,
                    ObservedAt   = x.ObservedAt,
                })
                .ToListAsync();

            return pending;
        }

        [AbpAuthorize(PermissionNames.Pages_PriceModeration)]
        [UnitOfWork]
        public virtual async Task ModerateAsync(ModeratePriceDto input)
        {
            var price = await _priceRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            if (price == null)
                throw new UserFriendlyException("Price submission not found");

            price.Status         = input.Status;
            price.ModerationNote = input.ModerationNote;

            await _priceRepository.UpdateAsync(price);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(PermissionNames.Pages_PriceModeration)]
        [UnitOfWork]
        public virtual async Task CreateApprovedAsync(SubmitPriceDto input)
        {
            var price = new Price
            {
                ProductId      = input.ProductId,
                StoreId        = input.StoreId,
                Amount         = input.Amount,
                OriginalAmount = input.OriginalAmount,
                Currency       = "INR",
                Source         = PriceSource.RetailerReported,
                Status         = PriceStatus.Approved,
                ObservedAt     = DateTime.UtcNow,
            };

            await _priceRepository.InsertAsync(price);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(PermissionNames.Pages_PriceModeration)]
        [UnitOfWork]
        public virtual async Task<PagedResultDto<AdminPriceDto>> GetAllAsync(PagedPriceRequestDto input)
        {
            var query = _priceRepository.GetAll()
                .Include(x => x.Product)
                .Include(x => x.Store)
                .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
                    x.Product.Name.ToLower().Contains(input.Keyword.ToLower()) ||
                    x.Store.Name.ToLower().Contains(input.Keyword.ToLower()))
                .WhereIf(input.StoreId.HasValue, x => x.StoreId == input.StoreId.Value)
                .WhereIf(input.ProductId.HasValue, x => x.ProductId == input.ProductId.Value)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value);

            var totalCount = await query.CountAsync();

            var prices = await query
                .OrderByDescending(x => x.ObservedAt)
                .PageBy(input)
                .Select(x => new AdminPriceDto
                {
                    Id             = x.Id,
                    ProductId      = x.ProductId,
                    ProductName    = x.Product.Name,
                    StoreId        = x.StoreId,
                    StoreName      = x.Store.Name,
                    Amount         = x.Amount,
                    OriginalAmount = x.OriginalAmount,
                    Status         = x.Status,
                    Source         = x.Source,
                    ObservedAt     = x.ObservedAt,
                })
                .ToListAsync();

            return new PagedResultDto<AdminPriceDto>(totalCount, prices);
        }

        [AbpAuthorize(PermissionNames.Pages_PriceModeration)]
        [UnitOfWork]
        public virtual async Task UpdateAsync(UpdatePriceDto input)
        {
            var price = await _priceRepository.GetAsync(input.Id);

            price.Amount         = input.Amount;
            price.OriginalAmount = input.OriginalAmount;
            price.Status         = input.Status;

            await _priceRepository.UpdateAsync(price);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(PermissionNames.Pages_PriceModeration)]
        [UnitOfWork]
        public virtual async Task DeleteAsync(EntityDto<Guid> input)
        {
            await _priceRepository.DeleteAsync(input.Id);
        }
    }
}
