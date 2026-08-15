using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using System.Linq;

namespace EnterpriseBase.Editions;

public class EditionManager : AbpEditionManager
{
    public const string DefaultEditionName = "Free";

    public IQueryable<EnterpriseEdition> EnterpriseEditions => Editions.OfType<EnterpriseEdition>();

    public EditionManager(
        IRepository<Edition> editionRepository,
        IAbpZeroFeatureValueStore featureValueStore,
        IUnitOfWorkManager unitOfWorkManager)
        : base(editionRepository, featureValueStore, unitOfWorkManager)
    {
    }
}
