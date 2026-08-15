using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using EnterpriseBase.Application.MasterData.Products.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.MasterData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.MasterData.Products
{
    // Admin-authenticated catalog management (create/edit product metadata).
    // This is NOT the public search/compare service shoppers hit - see
    // EnterpriseBase.Application.Pricing for the [AllowAnonymous] search
    // service that ranks Prices by Store. See DOCS/PRIZZOO_MIGRATION_NOTES.md.
    [AbpAuthorize(PermissionNames.Pages_Products)]
    public class ProductAppService : EnterpriseBaseAppServiceBase, IProductAppService
    {
        private readonly IRepository<Product, Guid> _repository;

        public ProductAppService(IRepository<Product, Guid> repository)
        {
            _repository = repository;
        }

        [UnitOfWork]
        public virtual async Task<ProductDto> GetAsync(EntityDto<Guid> input)
        {
            var product = await _repository.GetAll()
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .FirstOrDefaultAsync(x => x.Id == input.Id);

            if (product == null)
                throw new UserFriendlyException("Product not found");

            return MapToDto(product);
        }

        [UnitOfWork]
        public virtual async Task<ProductDto> GetByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return null;

            var product = await _repository.GetAll()
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .FirstOrDefaultAsync(x => x.Barcode == barcode);

            return product == null ? null : MapToDto(product);
        }

        [UnitOfWork]
        public virtual async Task<PagedResultDto<ProductDto>> GetAllAsync(PagedProductRequestDto input)
        {
            var query = _repository.GetAll()
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
                    x.Name.ToLower().Contains(input.Keyword.ToLower()) ||
                    (x.Barcode != null && x.Barcode.ToLower().Contains(input.Keyword.ToLower())))
                .WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId.Value)
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive.Value);

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(x => x.Name)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<ProductDto>(totalCount, products.Select(MapToDto).ToList());
        }

        [AbpAuthorize(PermissionNames.Pages_Products_Create)]
        [UnitOfWork]
        public virtual async Task<ProductDto> CreateAsync(CreateProductDto input)
        {
            if (!string.IsNullOrWhiteSpace(input.Barcode))
            {
                var barcodeExists = await _repository.FirstOrDefaultAsync(x => x.Barcode == input.Barcode);
                if (barcodeExists != null)
                    throw new UserFriendlyException("A product with this barcode already exists");
            }

            var product = new Product
            {
                Name        = input.Name,
                Barcode     = input.Barcode,
                Description = input.Description,
                CategoryId  = input.CategoryId,
                UnitId      = input.UnitId,
                ImageId     = input.ImageId,
                IsActive    = input.IsActive
            };

            await _repository.InsertAsync(product);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(new EntityDto<Guid>(product.Id));
        }

        [AbpAuthorize(PermissionNames.Pages_Products_Edit)]
        [UnitOfWork]
        public virtual async Task<ProductDto> UpdateAsync(UpdateProductDto input)
        {
            var product = await _repository.GetAsync(input.Id);

            product.Name        = input.Name;
            product.Barcode     = input.Barcode;
            product.Description = input.Description;
            product.CategoryId  = input.CategoryId;
            product.UnitId      = input.UnitId;
            product.ImageId     = input.ImageId;
            product.IsActive    = input.IsActive;

            await _repository.UpdateAsync(product);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(new EntityDto<Guid>(product.Id));
        }

        [AbpAuthorize(PermissionNames.Pages_Products_Delete)]
        [UnitOfWork]
        public virtual async Task DeleteAsync(EntityDto<Guid> input)
        {
            await _repository.DeleteAsync(input.Id);
        }

        public async Task<List<ComboboxItemDto>> GetForComboboxAsync()
        {
            var list = await _repository.GetAll()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return list.Select(x => new ComboboxItemDto
            {
                Value       = x.Id.ToString(),
                DisplayText = x.Name
            }).ToList();
        }

        private static ProductDto MapToDto(Product x)
        {
            return new ProductDto
            {
                Id           = x.Id,
                Name         = x.Name,
                Barcode      = x.Barcode,
                Description  = x.Description,
                CategoryId   = x.CategoryId,
                CategoryName = x.Category?.Name,
                UnitId       = x.UnitId,
                UnitName     = x.Unit?.Name,
                ImageId      = x.ImageId,
                IsActive     = x.IsActive
            };
        }
    }
}
