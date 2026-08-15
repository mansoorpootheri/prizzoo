using EnterpriseBase.Employees;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace EnterpriseBase.EntityFrameworkCore.Seed.Tenants;

public class DefaultEmployeeTypesCreator
{
    private readonly EnterpriseBaseDbContext _context;
    private readonly int _tenantId;

    private static readonly List<string> DefaultEmployeeTypes = new()
    {
        "Full-Time",
        "Part-Time",
        "Contract",
        "Intern",
        "Consultant"
    };

    public DefaultEmployeeTypesCreator(EnterpriseBaseDbContext context, int tenantId)
    {
        _context = context;
        _tenantId = tenantId;
    }

    public void Create()
    {
        var existing = _context.EmployeeTypes.IgnoreQueryFilters()
            .Where(e => e.TenantId == _tenantId)
            .Select(e => e.Name)
            .ToList();

        var toAdd = DefaultEmployeeTypes
            .Where(name => !existing.Contains(name))
            .Select(name => new EmployeeType
            {
                TenantId = _tenantId,
                Name = name,
                IsActive = true
            })
            .ToList();

        if (toAdd.Any())
        {
            _context.EmployeeTypes.AddRange(toAdd);
            _context.SaveChanges();
        }
    }
}
