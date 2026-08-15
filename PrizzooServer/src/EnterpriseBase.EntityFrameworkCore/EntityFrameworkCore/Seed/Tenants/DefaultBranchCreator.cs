using EnterpriseBase.Branches;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EnterpriseBase.EntityFrameworkCore.Seed.Tenants;

public class DefaultBranchCreator
{
    private readonly EnterpriseBaseDbContext _context;
    private readonly int _tenantId;

    public DefaultBranchCreator(EnterpriseBaseDbContext context, int tenantId)
    {
        _context = context;
        _tenantId = tenantId;
    }

    public void Create()
    {
        var exists = _context.Branches.IgnoreQueryFilters()
            .Any(b => b.TenantId == _tenantId && b.IsHeadOffice);

        if (exists) return;

        _context.Branches.Add(new Branch
        {
            TenantId     = _tenantId,
            BranchName   = "Head Office",
            BranchCode   = "HO",
            IsHeadOffice = true
        });

        _context.SaveChanges();
    }
}
