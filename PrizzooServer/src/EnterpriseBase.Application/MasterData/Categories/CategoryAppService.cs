using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using EnterpriseBase.Application.MasterData.Categories.Dto;
using EnterpriseBase.Authorization;
using EnterpriseBase.MasterData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.MasterData.Categories
{
    [AbpAuthorize(PermissionNames.Pages_Products_Categories)]
    public class CategoryAppService : EnterpriseBaseAppServiceBase, ICategoryAppService
    {
        private readonly IRepository<Category, Guid> _repository;

        public CategoryAppService(IRepository<Category, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var list = await _repository.GetAll().OrderBy(x => x.Name).ToListAsync();
            return list.Select(x => new CategoryDto
            {
                Id          = x.Id,
                Name        = x.Name,
                Code        = x.Code,
                Description = x.Description,
                IsActive    = x.IsActive
            }).ToList();
        }

        [AbpAuthorize(PermissionNames.Pages_Products_Categories_Create, PermissionNames.Pages_Products_Categories_Edit)]
        public async Task CreateOrEditAsync(CreateEditCategoryDto input)
        {
            if (!input.Id.HasValue || input.Id == Guid.Empty)
            {
                await _repository.InsertAsync(new Category
                {
                    Id          = Guid.NewGuid(),
                    Name        = input.Name,
                    Code        = input.Code,
                    Description = input.Description,
                    IsActive    = input.IsActive
                });
            }
            else
            {
                var entity = await _repository.GetAsync(input.Id.Value);
                entity.Name        = input.Name;
                entity.Code        = input.Code;
                entity.Description = input.Description;
                entity.IsActive    = input.IsActive;
            }
        }

        [AbpAuthorize(PermissionNames.Pages_Products_Categories_Delete)]
        public async Task DeleteAsync(EntityDto<Guid> input)
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
    }
}
