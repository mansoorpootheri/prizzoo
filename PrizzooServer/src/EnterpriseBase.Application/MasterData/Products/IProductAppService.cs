using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EnterpriseBase.Application.MasterData.Products.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.MasterData.Products
{
    public interface IProductAppService : IApplicationService
    {
        Task<ProductDto> GetAsync(EntityDto<Guid> input);
        Task<ProductDto> GetByBarcodeAsync(string barcode);
        Task<PagedResultDto<ProductDto>> GetAllAsync(PagedProductRequestDto input);
        Task<ProductDto> CreateAsync(CreateProductDto input);
        Task<ProductDto> UpdateAsync(UpdateProductDto input);
        Task DeleteAsync(EntityDto<Guid> input);
        Task<List<ComboboxItemDto>> GetForComboboxAsync();
    }
}
