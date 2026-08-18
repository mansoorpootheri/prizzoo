using EnterpriseBase.Geography;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EnterpriseBase.EntityFrameworkCore.Seed.Host;

/// <summary>
/// Country/State/District are fixed constants for MVP - Prizzoo launches in
/// Kerala, India, and doesn't need admin-editable Country/State master data
/// for that. Auto-loads India, Kerala, and all 14 Kerala districts on host
/// seed so the store-creation form's District ("City") picker has data
/// immediately, without a manual data-entry step. District already has its
/// own CRUD (DistrictAppService/Pages_Geography_Districts) if a district
/// ever needs editing later.
/// </summary>
public class DefaultGeographyCreator
{
    private static readonly string[] KeralaDistricts =
    {
        "Thiruvananthapuram", "Kollam", "Pathanamthitta", "Alappuzha",
        "Kottayam", "Idukki", "Ernakulam", "Thrissur", "Palakkad",
        "Malappuram", "Kozhikode", "Wayanad", "Kannur", "Kasaragod",
    };

    private readonly EnterpriseBaseDbContext _context;

    public DefaultGeographyCreator(EnterpriseBaseDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        var india = _context.Countries.IgnoreQueryFilters().FirstOrDefault(c => c.CountryName == "India");
        if (india == null)
        {
            india = new Country { CountryName = "India", IsoCode = "IN", PhoneCode = "+91" };
            _context.Countries.Add(india);
            _context.SaveChanges();
        }

        var kerala = _context.States.IgnoreQueryFilters().FirstOrDefault(s => s.CountryId == india.Id && s.StateName == "Kerala");
        if (kerala == null)
        {
            kerala = new State { StateName = "Kerala", StateCode = "KL", CountryId = india.Id };
            _context.States.Add(kerala);
            _context.SaveChanges();
        }

        var existingDistrictNames = _context.Districts.IgnoreQueryFilters()
            .Where(d => d.StateId == kerala.Id)
            .Select(d => d.DistrictName)
            .ToList();

        var toAdd = KeralaDistricts
            .Where(name => !existingDistrictNames.Contains(name))
            .Select(name => new District { DistrictName = name, StateId = kerala.Id });

        _context.Districts.AddRange(toAdd);
        _context.SaveChanges();
    }
}
