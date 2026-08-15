using Abp.Application.Editions;
using Abp.Application.Features;
using EnterpriseBase.Editions;
using EnterpriseBase.Features;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EnterpriseBase.EntityFrameworkCore.Seed.Host;

public class DefaultEditionCreator
{
    private readonly EnterpriseBaseDbContext _context;

    public DefaultEditionCreator(EnterpriseBaseDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        CreateEditions();
    }

    private void CreateEditions()
    {
        CreateFreeEdition();
        CreateStarterEdition();
        CreateBusinessEdition();
    }

    private void CreateFreeEdition()
    {
        var edition = GetOrCreateEdition("Free", "Free Plan", monthlyPrice: null, annualPrice: null, trialDays: 30);
        SetFeature(edition.Id, AppFeatures.MaxUserCount, "1");
        SetFeature(edition.Id, AppFeatures.MaxBranchCount, "1");
        SetFeature(edition.Id, AppFeatures.MaxTransactionEntries, "3");
        SetFeature(edition.Id, AppFeatures.HasAdvancedReports, "false");
        SetFeature(edition.Id, AppFeatures.HasApiAccess, "false");
        SetFeature(edition.Id, AppFeatures.HasMultiCurrency, "false");
    }

    private void CreateStarterEdition()
    {
        var edition = GetOrCreateEdition("Starter", "Starter Plan", monthlyPrice: 599m, annualPrice: 5990m, trialDays: null);
        SetFeature(edition.Id, AppFeatures.MaxUserCount, "5");
        SetFeature(edition.Id, AppFeatures.MaxBranchCount, "3");
        SetFeature(edition.Id, AppFeatures.MaxTransactionEntries, "0");
        SetFeature(edition.Id, AppFeatures.HasAdvancedReports, "true");
        SetFeature(edition.Id, AppFeatures.HasApiAccess, "false");
        SetFeature(edition.Id, AppFeatures.HasMultiCurrency, "true");
    }

    private void CreateBusinessEdition()
    {
        var edition = GetOrCreateEdition("Business", "Business Plan", monthlyPrice: 1499m, annualPrice: 14990m, trialDays: null);
        SetFeature(edition.Id, AppFeatures.MaxUserCount, "0");
        SetFeature(edition.Id, AppFeatures.MaxBranchCount, "0");
        SetFeature(edition.Id, AppFeatures.MaxTransactionEntries, "0");
        SetFeature(edition.Id, AppFeatures.HasAdvancedReports, "true");
        SetFeature(edition.Id, AppFeatures.HasApiAccess, "true");
        SetFeature(edition.Id, AppFeatures.HasMultiCurrency, "true");
    }

    private EnterpriseEdition GetOrCreateEdition(string name, string displayName, decimal? monthlyPrice, decimal? annualPrice, int? trialDays)
    {
        var edition = _context.Editions.IgnoreQueryFilters().OfType<EnterpriseEdition>().FirstOrDefault(e => e.Name == name);
        if (edition == null)
        {
            edition = new EnterpriseEdition
            {
                Name = name,
                DisplayName = displayName,
                MonthlyPrice = monthlyPrice,
                AnnualPrice = annualPrice,
                GstRate = 18m,
                IsPriceInclusiveOfGst = false,
                HsnSacCode = "998314",
                TrialDayCount = trialDays
            };
            _context.Editions.Add(edition);
            _context.SaveChanges();
        }
        return edition;
    }

    private void SetFeature(int editionId, string featureName, string value)
    {
        var existingFeature = _context.EditionFeatureSettings
            .IgnoreQueryFilters()
            .FirstOrDefault(ef => ef.EditionId == editionId && ef.Name == featureName);
            
        if (existingFeature == null)
        {
            _context.EditionFeatureSettings.Add(new EditionFeatureSetting
            {
                Name = featureName,
                Value = value,
                EditionId = editionId
            });
            _context.SaveChanges();
        }
    }
}
