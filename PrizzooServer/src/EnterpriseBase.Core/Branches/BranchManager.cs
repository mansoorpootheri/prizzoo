using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Branches;

public class BranchManager : DomainService
{
    private readonly IRepository<Branch, int> _branchRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchManager(IRepository<Branch, int> branchRepository, IHttpContextAccessor httpContextAccessor)
    {
        _branchRepository = branchRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetCurrentBranchId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("BranchIds");
        if (string.IsNullOrEmpty(claim?.Value))
            throw new UserFriendlyException("No branch assigned to current user. Please contact administrator.");
        var first = claim.Value.Split(',').FirstOrDefault();
        if (!int.TryParse(first, out var id))
            throw new UserFriendlyException("Invalid branch assignment for current user.");
        return id;
    }

    public async Task<List<ComboboxItemDto>> GetBranchesForCombobox()
    {
        var branchIdsClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("BranchIds")?.Value;
        var query = _branchRepository.GetAll().AsNoTracking();

        if (!string.IsNullOrEmpty(branchIdsClaim))
        {
            var branchIds = branchIdsClaim.Split(',').Select(int.Parse).ToList();
            query = query.Where(b => branchIds.Contains(b.Id));
        }

        var branches = await query.OrderBy(x => x.BranchName).ToListAsync();

        return branches.Select(x => new ComboboxItemDto
        {
            Value = x.Id.ToString(),
            DisplayText = x.BranchName
        }).ToList();
    }

    public async Task<List<ComboboxItemDto>> GetAllBranchesForCombobox()
    {
        var query = _branchRepository.GetAll().AsNoTracking();
        var branches = await query.OrderBy(x => x.BranchName).ToListAsync();

        return branches.Select(x => new ComboboxItemDto
        {
            Value = x.Id.ToString(),
            DisplayText = x.BranchName
        }).ToList();
    }

    public async Task<List<Branch>> GetAllBranches()
    {
        return await _branchRepository.GetAll().AsNoTracking().OrderBy(x => x.BranchName).ToListAsync();
    }

    public async Task<Branch> GetBranchById(int id)
    {
        return await _branchRepository.FirstOrDefaultAsync(x => x.Id == id);
    }
}
