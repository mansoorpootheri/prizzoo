using EnterpriseBase.MasterData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EnterpriseBase.EntityFrameworkCore.Seed.Host;

/// <summary>
/// A Location now needs coordinates to be usable anywhere (store form,
/// shopper LocationPickerModal - see LocationAppService.GetForComboboxAsync's
/// filter), but Latitude/Longitude are meant to be captured on-site via an
/// admin's "use my current location" action, not typed in. This seeds
/// approximate coordinates for the two Kozhikode-district Locations created
/// before that requirement existed, purely to unblock dev/testing without
/// needing to physically be in Feroke or Ramanattukara - an admin can always
/// re-capture more precisely later via LocationMaster, which this seeder
/// never overwrites (it only fills in Locations that are still null).
/// </summary>
public class DefaultLocationsCreator
{
    private static readonly (string Name, decimal Latitude, decimal Longitude)[] KozhikodeLocations =
    {
        ("Feroke", 11.1489m, 75.7827m),
        ("Ramanattukara", 11.1892m, 75.8067m),
    };

    private readonly EnterpriseBaseDbContext _context;

    public DefaultLocationsCreator(EnterpriseBaseDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        var kozhikode = _context.Districts.IgnoreQueryFilters()
            .FirstOrDefault(d => d.DistrictName == "Kozhikode");
        if (kozhikode == null)
            return; // DefaultGeographyCreator runs first and seeds this - defensive only.

        foreach (var (name, latitude, longitude) in KozhikodeLocations)
        {
            var location = _context.Locations.IgnoreQueryFilters()
                .FirstOrDefault(l => l.DistrictId == kozhikode.Id && l.Name == name);

            if (location == null)
            {
                _context.Locations.Add(new Location
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    DistrictId = kozhikode.Id,
                    Latitude = latitude,
                    Longitude = longitude,
                    IsActive = true,
                });
            }
            else if (location.Latitude == null || location.Longitude == null)
            {
                location.Latitude = latitude;
                location.Longitude = longitude;
            }
        }

        _context.SaveChanges();
    }
}
